using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointsCheckoutExtensionProvider : BaseCheckoutExtensionProvider {

        private readonly dynamic _shapeFactory;

        public PickupPointsCheckoutExtensionProvider(
            IShapeFactory shapeFactory)
            : base() {

            _shapeFactory = shapeFactory;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private const string Prefix = "PickupPointsCheckout";

        public override IEnumerable<AdditionalIndexShippingAddressViewModel> 
            AdditionalIndexShippingAddressShapes() {
            yield return new AdditionalIndexShippingAddressViewModel {
                TabTitle = T("Pickup Points").Text,
                TabId = "tab-pickup-points",
                NavId = "nav-pickup-points",
                TabShape = _shapeFactory.CheckoutPickupPointsIndexShape()
            };
        }

        public override void ProcessAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context) {

        }

    }
}