using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard.ContentManagement;
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
        private readonly IContentManager _contentManager;

        public PickupPointsCheckoutExtensionProvider(
            IShapeFactory shapeFactory,
            IContentManager contentManager)
            : base() {

            _shapeFactory = shapeFactory;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private const string Prefix = "PickupPointsCheckout";

        public override IEnumerable<AdditionalIndexShippingAddressViewModel> 
            AdditionalIndexShippingAddressShapes(CheckoutViewModel cvm) {

            // Collect all ContentItems that represent a PickupPoint. Info from the
            // current CheckoutViewModel may on principle be used to "filter" the
            // available PickupPoints.
            // TODO: set things up to filter pickup points based on the cvm
            var pPPoints = _contentManager
                .Query<PickupPointPart>(VersionOptions.Published, PickupPointPart.DefaultContentTypeName)
                .List();
            // Build a shape for each, in a specific "display" type, that will be
            // shown to the user to allow selecting one. (this will be handled in the
            // injected Index shape)
            yield return new AdditionalIndexShippingAddressViewModel {
                // we'll use this to identify the "selected" provider
                UniqueProviderId = "PickupPointsCheckout",
                TabTitle = T("Pickup Points").Text,
                TabId = "tab-pickup-points",
                NavId = "nav-pickup-points",
                TabShape = _shapeFactory.CheckoutPickupPointsIndexShape(
                    PickupPoints: pPPoints,
                    ContentManager: _contentManager,
                    Prefix: Prefix)
            };
        }

        public override void ProcessAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context) {

        }

    }
}