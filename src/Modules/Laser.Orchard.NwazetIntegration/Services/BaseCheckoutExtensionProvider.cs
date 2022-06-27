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

        public virtual IEnumerable<dynamic> AdditionalCheckoutStartShapes() {
            yield break;
        }

        public virtual IEnumerable<AdditionalIndexShippingAddressViewModel> AdditionalIndexShippingAddressShapes() {
            yield break;
        }

        public virtual void ProcessAdditionalCheckoutStartInformation(
            CheckoutExtensionContext context) { }

        public virtual void ProcessAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context) {}
    }
}