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

        private const string ProviderId = "PickupPointsCheckout";
        // constants for the form
        private const string Prefix = "PickupPointsCheckout";
        private const string SelectedPointInputName = Prefix + ".SelectedPickupPoint";
        private const string SelectedPointInputBaseId = Prefix + "_SelectedPickupPoint";


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
                UniqueProviderId = ProviderId,
                TabTitle = T("Pickup Points").Text,
                TabId = "tab-pickup-points",
                NavId = "nav-pickup-points",
                TabShape = _shapeFactory.CheckoutPickupPointsIndexShape(
                    PickupPoints: pPPoints,
                    ContentManager: _contentManager,
                    Prefix: Prefix,
                    SelectedPointInputName: SelectedPointInputName,
                    SelectedPointInputBaseId: SelectedPointInputBaseId
                    )
            };
        }

        public override bool IsSelectedProviderForIndex(string providerId) {
            return ProviderId.Equals(providerId, StringComparison.OrdinalIgnoreCase);
        }

        public override bool ValidateAdditionalIndexShippingAddressInformation(
            CheckoutViewModel cvm) {
            return base.ValidateAdditionalIndexShippingAddressInformation(cvm);
        }

        public override void ProcessAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context, CheckoutViewModel cvm) {
            // if on post we find that the user was selecting a pickup point,
            // figure out which one, and store that information in a serialized
            // way that'll make it easy to carry between actions.

            // We are going to store the "selected" pickup point in the viewmodel
            // even if the user isn't, strictly speaking, going to select a pickup
            // point, so that in terms of UX the use will see the correct point
            // select when they look for it.
            // Of course this only works if that portion of the form is actually
            // sumitted: otherwise there is no way to store that information and 
            // realize that kind of UX.

            var fieldName = SelectedPointInputName;
            var valueResult = context.ValueProvider.GetValue(fieldName);
            // If we haven't received the value from the request, do nothing.
            if (valueResult != null) {
                // the value is the Id if the ContentItem of the selected PickupPoint
            }
        }

    }
}