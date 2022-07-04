using Laser.Orchard.NwazetIntegration.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    /// <summary>
    /// Base astract implementation so we don't have to reimplement empty
    /// methods in every implementation of the provider.
    /// </summary>
    public abstract class BaseCheckoutExtensionProvider
        : ICheckoutExtensionProvider {

        protected BaseCheckoutExtensionProvider() { }

        #region Extensions for CheckoutStart
        public virtual IEnumerable<dynamic> AdditionalCheckoutStartShapes() {
            yield break;
        }

        public virtual void ProcessAdditionalCheckoutStartInformation(
            CheckoutExtensionContext context) { }
        #endregion

        #region Extensions for Index
        public virtual IEnumerable<AdditionalIndexShippingAddressViewModel> 
            AdditionalIndexShippingAddressShapes(CheckoutViewModel cvm) {
            yield break;
        }

        public virtual bool IsSelectedProviderForIndex(string providerId) {
            return false;
        }

        public virtual bool ValidateAdditionalIndexShippingAddressInformation(
            CheckoutViewModel cvm) {
            return true;
        }

        public virtual void ProcessAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context, CheckoutViewModel cvm) {}
        #endregion
    }
}