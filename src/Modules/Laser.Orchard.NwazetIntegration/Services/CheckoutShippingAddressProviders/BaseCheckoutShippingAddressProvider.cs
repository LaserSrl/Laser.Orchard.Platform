using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.NwazetIntegration.ViewModels;

namespace Laser.Orchard.NwazetIntegration.Services.CheckoutShippingAddressProviders {
    public abstract class BaseCheckoutShippingAddressProvider
        : ICheckoutShippingAddressProvider {

        protected BaseCheckoutShippingAddressProvider() { }

        public virtual IEnumerable<AdditionalIndexShippingAddressViewModel> 
            GetIndexShippingAddressShapes(CheckoutViewModel cvm) {
            yield break;
        }

        public virtual int GetShippingCountryId(CheckoutViewModel cvm) {
            return 0;
        }

        public virtual int GetShippingProvinceId(CheckoutViewModel cvm) {
            return 0;
        }

        public virtual int GetShippingCityId(CheckoutViewModel cvm) {
            return 0;
        }

        public virtual string GetShippingCountryName(CheckoutViewModel cvm) {
            return string.Empty;
        }

        public virtual string GetShippingPostalCode(CheckoutViewModel cvm) {
            return null;
        }

        public virtual bool IsSelectedProviderForIndex(string providerId) {
            return false;
        }

        public virtual bool ProcessAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context, CheckoutViewModel cvm) {
            return true;
        }

        public virtual bool ValidateAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context, CheckoutViewModel cvm) {
            return true;
        }

        public virtual void ReinflateShippingAddress(ShippingAddressReinflationContext context) { }

        public virtual void ReinflateViewModel(CheckoutViewModel viewModel) { }
    }
}