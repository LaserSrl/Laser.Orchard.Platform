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

namespace Laser.Orchard.NwazetIntegration.Services.CheckoutShippingAddressProviders {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointsCheckoutShippingAddressProvider 
        : BaseCheckoutShippingAddressProvider {

        private readonly dynamic _shapeFactory;
        private readonly IContentManager _contentManager;

        public PickupPointsCheckoutShippingAddressProvider(
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
            GetIndexShippingAddressShapes(CheckoutViewModel cvm) {

            // Collect all ContentItems that represent a PickupPoint. Info from the
            // current CheckoutViewModel may on principle be used to "filter" the
            // available PickupPoints.
            // TODO: set things up to filter pickup points based on the cvm
            var pPPoints = _contentManager
                .Query<PickupPointPart>(VersionOptions.Published, PickupPointPart.DefaultContentTypeName)
                .List();
            // get existing view model for pickup points (if any)
            var viewModel = cvm.ProviderViewModels.ContainsKey(ProviderId)
                ? (PickupPointsCheckoutViewModel)cvm.ProviderViewModels[ProviderId]
                : new PickupPointsCheckoutViewModel();

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
                    ViewModel: viewModel,
                    ContentManager: _contentManager,
                    Prefix: Prefix,
                    SelectedPointInputName: SelectedPointInputName,
                    SelectedPointInputBaseId: SelectedPointInputBaseId
                    )
            };
        }

        public override bool IsSelectedProviderForIndex(string providerId) {
            return ProviderId
                .Equals(providerId, StringComparison.OrdinalIgnoreCase);
        }

        public override bool ValidateAdditionalIndexShippingAddressInformation(
            CheckoutViewModel cvm) {

            var viewModel = cvm.ProviderViewModels.ContainsKey(ProviderId)
                ? (PickupPointsCheckoutViewModel)cvm.ProviderViewModels[ProviderId]
                : new PickupPointsCheckoutViewModel();

            var part = viewModel.PickupPointPart;
            // The information for the pickup point should be validated when it is
            // created/updated. Here it's being used in front-end, so we assume it 
            // to be valid.
            var valid = part != null;
            // There may be other validation steps involving interactions between
            // selected points and other stuff.
            // TODO: make this validation extensible

            return valid;
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

            // get existing view model for pickup points (if any)
            var viewModel = cvm.ProviderViewModels.ContainsKey(ProviderId)
                ? (PickupPointsCheckoutViewModel)cvm.ProviderViewModels[ProviderId]
                : new PickupPointsCheckoutViewModel();

            var fieldName = SelectedPointInputName;
            var valueResult = context.ValueProvider.GetValue(fieldName);
            // If we haven't received the value from the request, do nothing.
            if (valueResult != null) {
                // the value is the Id if the ContentItem of the selected PickupPoint
                var idString = valueResult.AttemptedValue;
                var pickupPointId = 0;
                if (int.TryParse(idString, out pickupPointId)) {

                    var selectedPart = _contentManager
                        .Get<PickupPointPart>(pickupPointId);
                    if (selectedPart != null) {
                        viewModel.SelectedPickupPointId = pickupPointId;
                        viewModel.PickupPointPart = selectedPart;
                    }
                }
            }

            // update the CheckoutViewModel with the information for the provider
            if (cvm.ProviderViewModels.ContainsKey(ProviderId)) {
                cvm.ProviderViewModels[ProviderId] = viewModel;
            } else {
                cvm.ProviderViewModels.Add(ProviderId, viewModel);
            }
        }

        public override int GetShippingCountryId(CheckoutViewModel cvm) {
            // get existing view model for pickup points (if any)
            var viewModel = cvm.ProviderViewModels.ContainsKey(ProviderId)
                ? (PickupPointsCheckoutViewModel)cvm.ProviderViewModels[ProviderId]
                : new PickupPointsCheckoutViewModel();
            if (viewModel.PickupPointPart != null) {
                return viewModel.PickupPointPart.CountryId;
            }
            return base.GetShippingCountryId(cvm);
        }

        public override string GetShippingPostalCode(CheckoutViewModel cvm) {
            // get existing view model for pickup points (if any)
            var viewModel = cvm.ProviderViewModels.ContainsKey(ProviderId)
                ? (PickupPointsCheckoutViewModel)cvm.ProviderViewModels[ProviderId]
                : new PickupPointsCheckoutViewModel();
            if (viewModel.PickupPointPart != null) {
                return viewModel.PickupPointPart.PostalCode;
            }
            return base.GetShippingPostalCode(cvm);
        }
    }
}