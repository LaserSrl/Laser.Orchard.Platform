using Laser.Orchard.NwazetIntegration.Aspects;
using Laser.Orchard.NwazetIntegration.Services.CheckoutShippingAddressProviders;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Models {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointOrderPart
        : ContentPart<PickupPointOrderPartRecord>,
        IOrderExtensionAspect {

        public string CountryName {
            get { return Retrieve(r => r.CountryName); }
            set { Store(r => r.CountryName, value); }
        }

        public int CountryId {
            get { return Retrieve(r => r.CountryId); }
            set { Store(r => r.CountryId, value); }
        }

        public string ProvinceName {
            get { return Retrieve(r => r.ProvinceName); }
            set { Store(r => r.ProvinceName, value); }
        }

        public int ProvinceId {
            get { return Retrieve(r => r.ProvinceId); }
            set { Store(r => r.ProvinceId, value); }
        }

        public string CityName {
            get { return Retrieve(r => r.CityName); }
            set { Store(r => r.CityName, value); }
        }

        public int CityId {
            get { return Retrieve(r => r.CityId); }
            set { Store(r => r.CityId, value); }
        }

        public string AddressLine1 {
            get { return Retrieve(r => r.AddressLine1); }
            set { Store(r => r.AddressLine1, value); }
        }
        public string AddressLine2 {
            get { return Retrieve(r => r.AddressLine2); }
            set { Store(r => r.AddressLine2, value); }
        }
        public string PostalCode {
            get { return Retrieve(r => r.PostalCode); }
            set { Store(r => r.PostalCode, value); }
        }

        public string AddressText(bool withCountry = true) {
            var strElements = new List<string>();
            if (!string.IsNullOrWhiteSpace(AddressLine1)) {
                strElements.Add(AddressLine1);
            }
            if (!string.IsNullOrWhiteSpace(AddressLine2)) {
                strElements.Add(AddressLine2);
            }
            if (!string.IsNullOrWhiteSpace(CityName)) {
                strElements.Add(CityName);
            }

            if (!string.IsNullOrWhiteSpace(ProvinceName)) {
                if (withCountry && !string.IsNullOrWhiteSpace(CountryName)) {
                    strElements.Add($"{ProvinceName} ({CountryName})");
                } else {
                    strElements.Add(ProvinceName);
                }
            } else if (withCountry && !string.IsNullOrWhiteSpace(CountryName)) {
                // edit the last element of the list so it has the country to it
                // to prevent a comma separator where it wouldn't be required
                var last = strElements.Last();
                strElements.RemoveAt(strElements.Count - 1);
                last = $"{last} ({CountryName})";
                strElements.Add(last);
            }


            if (!string.IsNullOrWhiteSpace(PostalCode)) {
                strElements.Add(PostalCode);
            }

            return string.Join(", ", strElements);
        }
        
        // Flag telling whether a pickup point had been selected for the
        // order this part is attached to.
        public virtual bool IsOrderPickupPoint {
            get { return Retrieve(r => r.IsOrderPickupPoint); }
            set { Store(r => r.IsOrderPickupPoint, value); }
        }

        public void ExtendCreation(CheckoutViewModel cvm) {
            // If the Order is addressed to a pickup point, then flag this
            // part and copy the address.
            if (cvm.ShippingRequired 
                && PickupPointsCheckoutShippingAddressProvider.ProviderId
                    .Equals(cvm.SelectedShippingAddressProviderId)
                ) {

                IsOrderPickupPoint = true;
                // figure out the pickup point part that was selected
                var viewModel = cvm.ProviderViewModels
                        .ContainsKey(PickupPointsCheckoutShippingAddressProvider.ProviderId)
                    ? (PickupPointsCheckoutViewModel)cvm
                        .ProviderViewModels[PickupPointsCheckoutShippingAddressProvider.ProviderId]
                    : new PickupPointsCheckoutViewModel();
                var selectedPart = viewModel.PickupPointPart;
                if (selectedPart != null) {
                    // copy the information from the pickup point part
                    CountryName = selectedPart.CountryName;
                    CountryId = selectedPart.CountryId;
                    ProvinceName = selectedPart.ProvinceName;
                    ProvinceId = selectedPart.ProvinceId;
                    CityName = selectedPart.CityName;
                    CityId = selectedPart.CityId;
                    AddressLine1 = selectedPart.AddressLine1;
                    AddressLine2 = selectedPart.AddressLine2;
                    PostalCode = selectedPart.PostalCode;
                }
            }
        }
    }
}