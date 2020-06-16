using Laser.Orchard.NwazetIntegration.Models;
using Newtonsoft.Json;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class CheckoutViewModel {
        public CheckoutViewModel() {
            ListAvailableShippingAddress = new List<AddressRecord>();
            ListAvailableBillingAddress = new List<AddressRecord>();

            AvailableShippingOptions = new List<ShippingOption>();
        }

        public ICurrencyProvider CurrencyProvider;

        #region Addresses
        public AddressesVM AsAddressesVM() {
            return new AddressesVM {
                ShippingAddress = ShippingAddress,
                ShippingAddressVM = ShippingAddressVM,
                BillingAddress = BillingAddress,
                BillingAddressVM = BillingAddressVM,
                Email = Email,
                Phone = Phone,
                SpecialInstructions = SpecialInstructions,
                ListAvailableShippingAddress = ListAvailableShippingAddress,
                ListAvailableBillingAddress = ListAvailableBillingAddress
            };
        }
        public void SetAddressesVM(AddressesVM vm) {
            ShippingAddress = vm.ShippingAddress;
            ShippingAddressVM = vm.ShippingAddressVM;
            BillingAddress = vm.BillingAddress;
            BillingAddressVM = vm.BillingAddressVM;
            Email = vm.Email;
            Phone = vm.Phone;
            SpecialInstructions = vm.SpecialInstructions;
            ListAvailableShippingAddress = vm.ListAvailableShippingAddress;
            ListAvailableBillingAddress = vm.ListAvailableBillingAddress;
        }
        public Address ShippingAddress { get; set; }
        public AddressEditViewModel ShippingAddressVM { get; set; }
        public Address BillingAddress { get; set; }
        public AddressEditViewModel BillingAddressVM { get; set; }
        public string Email { get; set; }
        public string PhonePrefix { get; set; }
        public string Phone { get; set; }
        public string SpecialInstructions { get; set; }
        [JsonIgnore]
        public List<AddressRecord> ListAvailableShippingAddress { get; set; }
        [JsonIgnore]
        public List<AddressRecord> ListAvailableBillingAddress { get; set; }
        private const string AddressEncryptionPurpose = "Serialize Address Information";
        public string SerializedAddresses { get; set; }
        public string EncodeAddresses() {

            SerializedAddresses = Convert.ToBase64String(
                MachineKey.Protect(
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(AsAddressesVM())),
                    AddressEncryptionPurpose
                    ));
            return SerializedAddresses;
        }
        public void DecodeAddresses() {
            var bytes = Convert.FromBase64String(SerializedAddresses);
            var unprotected = MachineKey.Unprotect(bytes, AddressEncryptionPurpose);
            if (unprotected != null) {
                SetAddressesVM(
                    JsonConvert.DeserializeObject<AddressesVM>(
                        Encoding.UTF8.GetString(unprotected)));
            }
        }
        public bool ResetAddresses { get; set; }
        #endregion

        #region Shipping
        public bool ShippingRequired { get; set; }
        public List<ShippingOption> AvailableShippingOptions { get; set; }
        /// <summary>
        /// This is ShippingOption.FormValue for the selected option, used to pull it
        /// from the form. This is called "ShippingOption" for retrocompatibility and
        /// to allow reusing pre-existing shapes.
        /// </summary>
        public string ShippingOption { get; set; }
        public ShippingOption SelectedShippingOption { get; set; }
        public bool ResetShipping { get; set; }
        #endregion

        #region Payments
        public IEnumerable<IPosService> PosServices { get; set; }
        /// <summary>
        /// Will contian the name of the selected PosService when the user
        /// selects it from the form.
        /// </summary>
        public string SelectedPosService { get; set; }
        #endregion
    }
}