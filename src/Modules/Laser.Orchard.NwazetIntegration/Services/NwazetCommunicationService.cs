using AutoMapper;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.NwazetIntegration.Models;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class NwazetCommunicationService : INwazetCommunicationService {

        private readonly IOrchardServices _orchardServices;
        private readonly ICommunicationService _communicationService;
        private readonly IRepository<AddressRecord> _addressRecord;
        private readonly IRepository<CommunicationSmsRecord> _repositoryCommunicationSmsRecord;


        public ILogger _Logger { get; set; }

        public NwazetCommunicationService(
            IOrchardServices orchardServices
            , ICommunicationService communicationService
            , IRepository<AddressRecord> addressRecord
            , IRepository<CommunicationSmsRecord> repositoryCommunicationSmsRecord) {
            _orchardServices = orchardServices;
            _communicationService = communicationService;
            _addressRecord = addressRecord;
            _repositoryCommunicationSmsRecord = repositoryCommunicationSmsRecord;
            _Logger = NullLogger.Instance;

        }
        private List<AddressRecord> GetAddressByUser(IUser user, AddressRecordType type) {
            if (user.Id > 0) {
                var contactpart = _communicationService.GetContactFromUser(user.Id);
                if (contactpart == null) { // non dovrebbe mai succedere (inserito nel caso cambiassimo la logica già implementata)
                    _communicationService.UserToContact(user);
                    contactpart = _communicationService.GetContactFromUser(user.Id);
                }
                if (contactpart != null) {
                    return _addressRecord
                        .Fetch(c =>
                            c.AddressType == type
                            && c.NwazetContactPartRecord_Id == contactpart.Id)
                        .OrderByDescending(z => z.TimeStampUTC)
                        .ToList();
                }
            }
            return new List<AddressRecord>();
        }

        public List<AddressRecord> GetShippingByUser(IUser user) {
            return GetAddressByUser(user, AddressRecordType.ShippingAddress);
        }

        public List<AddressRecord> GetBillingByUser(IUser user) {
            return GetAddressByUser(user, AddressRecordType.BillingAddress);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns>string[2] prefix e sms</returns>
        public string[] GetPhone(IUser user) {
            var contactpart = _communicationService.GetContactFromUser(user.Id);
            if (contactpart == null) { // non dovrebbe mai succedere (inserito nel caso cambiassimo la logica già implementata)
                _communicationService.UserToContact(user);
                contactpart = _communicationService.GetContactFromUser(user.Id);
            }
            if (contactpart == null)
                return new string[2];
            else {
                var csr = _repositoryCommunicationSmsRecord.Fetch(x => x.SmsContactPartRecord_Id == contactpart.Id).FirstOrDefault();
                if (csr != null) {
                    return new string[] { csr.Prefix, csr.Sms };
                }
            }
            return new string[2];
        }

        public void OrderToContact(OrderPart order) {
            // tutto in try catch perchè viene scatenato appena finito il pagamento e quindi non posso permettermi di annullare la transazione
            try {
                // recupero il contatto
                var currentUser = _orchardServices.WorkContext.CurrentUser;
                var ContactList = new List<ContentItem>();
                if (currentUser != null) {
                    var contactpart = _communicationService.GetContactFromUser(currentUser.Id);
                    if (contactpart == null) { // non dovrebbe mai succedere (inserito nel caso cambiassimo la logica già implementata)
                        _communicationService.UserToContact(currentUser);
                        contactpart = _communicationService.GetContactFromUser(currentUser.Id);
                    }
                    ContactList.Add(contactpart.ContentItem);
                } else {
                    var contacts = _communicationService.GetContactsFromMail(order.CustomerEmail);
                    if (contacts.Count > 0) {
                        ContactList = contacts;
                    } else {
                        var newcontact = _orchardServices.ContentManager.Create("CommunicationContact", VersionOptions.Draft);
                        ((dynamic)newcontact).CommunicationContactPart.Master = false;
                        ContactList.Add(newcontact);
                    }
                }
                var addressPart = order.As<AddressOrderPart>();
                Action<ContentItem> storeBilling = (ci) =>
                    StoreAddress(order.BillingAddress, addressPart, AddressRecordType.BillingAddress, ci);
                Action<ContentItem> storeShipping = (ci) =>
                    StoreAddress(order.ShippingAddress, addressPart, AddressRecordType.ShippingAddress, ci);

                foreach (var contactItem in ContactList) {
                    // nel caso in cui una sincro fallisce continua con 
                    try {
                        storeBilling(contactItem);
                    } catch (Exception ex) {
                        _Logger.Error("OrderToContact -> BillingAddress -> order id= " + order.Id.ToString() + " Error: " + ex.Message);
                    }
                    try {
                        storeShipping(contactItem);
                    } catch (Exception ex) {
                        _Logger.Error("OrderToContact -> ShippingAddress -> order id= " + order.Id.ToString() + " Error: " + ex.Message);
                    }
                    try {
                        _communicationService.AddEmailToContact(order.CustomerEmail, contactItem);
                    } catch (Exception ex) {
                        _Logger.Error("OrderToContact -> AddEmailToContact -> order id= " + order.Id.ToString() + " Error: " + ex.Message);
                    }
                    try { // non sovrascrivo se dato già presente
                        _communicationService.AddSmsToContact((order.CustomerPhone + ' ').Split(' ')[0], (order.CustomerPhone + ' ').Split(' ')[1], contactItem, false);
                    } catch (Exception ex) {
                        _Logger.Error("OrderToContact -> AddSmsToContact -> order id= " + order.Id.ToString() + " Error: " + ex.Message);
                    }
                }
            } catch (Exception myex) {
                _Logger.Error("OrderToContact -> order id= " + order.Id.ToString() + " Error: " + myex.Message);
            }
        }

        private void StoreAddress(Address address, AddressOrderPart addressPart, AddressRecordType addressType, ContentItem contact) {

            Mapper.Initialize(cfg => {
                cfg.CreateMap<Address, AddressRecord>();
            });
            var addressToStore = new AddressRecord();
            Mapper.Map<Address, AddressRecord>(address, addressToStore);
            if (addressPart != null) {
                switch (addressType) {
                    case AddressRecordType.ShippingAddress:
                        addressToStore.CountryId = addressPart.ShippingCountryId;
                        addressToStore.CityId = addressPart.ShippingCityId;
                        addressToStore.ProvinceId = addressPart.ShippingProvinceId;
                        addressToStore.Country = addressPart.ShippingCountryName;
                        addressToStore.City = addressPart.ShippingCityName;
                        addressToStore.Province = addressPart.ShippingProvinceName;
                        break;
                    case AddressRecordType.BillingAddress:
                        addressToStore.CountryId = addressPart.BillingCountryId;
                        addressToStore.CityId = addressPart.BillingCityId;
                        addressToStore.ProvinceId = addressPart.BillingProvinceId;
                        addressToStore.Country = addressPart.BillingCountryName;
                        addressToStore.City = addressPart.BillingCityName;
                        addressToStore.Province = addressPart.BillingProvinceName;
                        break;
                    default:
                        break;
                }
            }
            addressToStore.AddressType = addressType;
            StoreAddress(addressToStore, contact);
        }

        private void StoreAddress(AddressRecord addressToStore, ContentItem contact) {
            addressToStore.NwazetContactPartRecord_Id = contact.Id;
            bool AddNewAddress = true;
            foreach (var existingAddressRecord in contact.As<NwazetContactPart>().NwazetAddressRecord) {
                if (addressToStore.Id == existingAddressRecord.Id
                    || addressToStore.Equals(existingAddressRecord)) {
                    AddNewAddress = false;
                    existingAddressRecord.TimeStampUTC = DateTime.UtcNow;
                    // little trick to cause nHibernate to "replace" the old address
                    // with the updated one:
                    addressToStore.Id = existingAddressRecord.Id;
                    _addressRecord.Update(addressToStore);
                    _addressRecord.Flush();
                }
            }
            if (AddNewAddress) {
                _addressRecord.Create(addressToStore);
                _addressRecord.Flush();
            }
        }

        public AddressRecord GetAddress(int id) {
            return _addressRecord.Get(id);
        }
        public AddressRecord GetAddress(int id, IUser user) {
            var contactpart = ContactFromUser(user);
            if (contactpart == null) {
                return null;
            }
            var result = GetAddress(id);

            return result.NwazetContactPartRecord_Id == contactpart.Id
                ? result : null; ;
        }

        public void DeleteAddress(int id) {
            var address = GetAddress(id);
            Delete(address);
        }
        public void DeleteAddress(int id, IUser user) {
            var address = GetAddress(id, user);
            Delete(address);
        }
        private void Delete(AddressRecord address) {
            if (address != null) {
                _addressRecord.Delete(address);
            }
        }

        public void AddAddress(AddressRecord newAddress, IUser user) {
            var contactPart = ContactFromUser(user);
            StoreAddress(newAddress, contactPart.ContentItem);
        }

        private CommunicationContactPart ContactFromUser(IUser user) {
            if (user == null) {
                throw new ArgumentNullException("user");
            }
            var contactpart = _communicationService.GetContactFromUser(user.Id);
            if (contactpart == null) { // non dovrebbe mai succedere (inserito nel caso cambiassimo la logica già implementata)
                _communicationService.UserToContact(user);
                contactpart = _communicationService.GetContactFromUser(user.Id);
            }
            return contactpart;
        }
    }
}