using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointsCheckoutExtensionProvider : BaseCheckoutExtensionProvider {

        private const string Prefix = "PickupPointsCheckout";

        public override IEnumerable<AdditionalIndexShippingAddressViewModel> 
            AdditionalIndexShippingAddressShapes() {
            yield break;
        }

        public override void ProcessAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context) {

        }

    }
}