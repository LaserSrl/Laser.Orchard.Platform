using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Newtonsoft.Json.Linq;
using Nwazet.Commerce.Models;
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
        
        private readonly IContentManager _contentManager;
        private readonly IAddressConfigurationService _addressConfigurationService;

        public PickupPointsCheckoutShippingAddressProvider(
            IShapeFactory shapeFactory,
            IContentManager contentManager,
            IAddressConfigurationService addressConfigurationService)
            : base(shapeFactory) {
            
            _contentManager = contentManager;
            _addressConfigurationService = addressConfigurationService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public const string ProviderId = "PickupPointsCheckout";
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
                // TODO: extend this to other content types
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

        public override IEnumerable<AdditionalCheckoutShippingAddressSummaryViewModel> 
            GetSummaryShippingAddressShapes(CheckoutViewModel cvm) {

            if (IsSelectedProviderForIndex(cvm.SelectedShippingAddressProviderId)) {
                // Get the selected PickupPoint and prepare a shape to display it
                // in summaries across the checkout process.
                var viewModel = cvm.ProviderViewModels.ContainsKey(ProviderId)
                    ? (PickupPointsCheckoutViewModel)cvm.ProviderViewModels[ProviderId]
                    : new PickupPointsCheckoutViewModel();
                var selectedPart = viewModel.PickupPointPart;
                if (selectedPart != null) {
                    yield return new AdditionalCheckoutShippingAddressSummaryViewModel {
                        UniqueProviderId = ProviderId,
                        TabShape = _shapeFactory.CheckoutPickupPointAddressSummaryShape(
                            PickupPointPart: selectedPart
                        )
                    };
                }
            }
        }

        public override bool IsSelectedProviderForIndex(string providerId) {
            return ProviderId
                .Equals(providerId, StringComparison.OrdinalIgnoreCase);
        }

        public override bool ValidateAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context, CheckoutViewModel cvm) {

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

        public override bool ProcessAdditionalIndexShippingAddressInformation(
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
            var partFound = false;
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
                        partFound = true;
                    }
                }
            }

            // update the CheckoutViewModel with the information for the provider
            if (cvm.ProviderViewModels.ContainsKey(ProviderId)) {
                cvm.ProviderViewModels[ProviderId] = viewModel;
            } else {
                cvm.ProviderViewModels.Add(ProviderId, viewModel);
            }

            if (IsSelectedProviderForIndex(cvm.SelectedShippingAddressProviderId)) {
                return partFound;
            } else {
                return true;
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

        public override int GetShippingProvinceId(CheckoutViewModel cvm) {
            // get existing view model for pickup points (if any)
            var viewModel = cvm.ProviderViewModels.ContainsKey(ProviderId)
                ? (PickupPointsCheckoutViewModel)cvm.ProviderViewModels[ProviderId]
                : new PickupPointsCheckoutViewModel();
            if (viewModel.PickupPointPart != null) {
                return viewModel.PickupPointPart.ProvinceId;
            }
            return base.GetShippingProvinceId(cvm);
        }

        public override int GetShippingCityId(CheckoutViewModel cvm) {
            // get existing view model for pickup points (if any)
            var viewModel = cvm.ProviderViewModels.ContainsKey(ProviderId)
                ? (PickupPointsCheckoutViewModel)cvm.ProviderViewModels[ProviderId]
                : new PickupPointsCheckoutViewModel();
            if (viewModel.PickupPointPart != null) {
                return viewModel.PickupPointPart.CityId;
            }
            return base.GetShippingCityId(cvm);
        }

        public override string GetShippingCountryName(CheckoutViewModel cvm) {
            var countryId = GetShippingCountryId(cvm);
            if (countryId > 0) {
                var country = _addressConfigurationService
                    ?.GetCountry(countryId);
                if (country != null) {
                    return _contentManager.GetItemMetadata(country).DisplayText;
                }
            }

            return base.GetShippingCountryName(cvm);
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

        public override void ReinflateShippingAddress(ShippingAddressReinflationContext context) {
            // The goal is to have the information correctly populated in the target viewmodel.
            // Part of it may already be there. At any point we may find that the information in
            // the target viewmodel is wrong/broken: in that case we try to start over by copying
            // the info from the source viewmodel.

            // This is the object that will represent information for this provider in the end of
            // this process, regardless of where the information is from.
            var inflatedInfoObject = InflatePickupPointsVM(context.TargetCheckoutViewModel);
            // Were we successfull?
            if (inflatedInfoObject.PickupPointPart == null) {
                // Try to inflate from source.
                // Memorize the target Id to restore it in case we also fail to inflate from source
                var targetId = inflatedInfoObject.SelectedPickupPointId;
                inflatedInfoObject = InflatePickupPointsVM(context.SourceCheckoutViewModel);
                if (inflatedInfoObject.PickupPointPart == null) {
                    if (targetId > 0) {
                        inflatedInfoObject.SelectedPickupPointId = targetId;
                    }
                }
            }

            // Finally, the target viewmodel will contain the up to date information from this provider
            if (!context.TargetCheckoutViewModel.ProviderViewModels.ContainsKey(ProviderId)) {
                context.TargetCheckoutViewModel.ProviderViewModels.Add(
                    ProviderId, inflatedInfoObject);
            } else {
                context.TargetCheckoutViewModel.ProviderViewModels[ProviderId] = inflatedInfoObject;
            }
            // In case this provider is the actively selected one, we should be also reinflating 
            // the ShippingAddress property of the CheckoutViewmodel. This is the object Nwazet 
            // will try to understand as an address.
            if (IsSelectedProviderForIndex(context.TargetCheckoutViewModel.SelectedShippingAddressProviderId)) {
                if (inflatedInfoObject != null && inflatedInfoObject.PickupPointPart != null) {
                    // Some information is not available on PickupPoints, so we take it from the
                    // BillingAddress, since that is always mandatory.
                    context.TargetCheckoutViewModel.ShippingAddress = new Address {
                        FirstName = context.TargetCheckoutViewModel.BillingAddress.FirstName,
                        LastName = context.TargetCheckoutViewModel.BillingAddress.LastName,
                        Address1 = inflatedInfoObject.PickupPointPart.AddressLine1,
                        Address2 = inflatedInfoObject.PickupPointPart.AddressLine2,
                        City = inflatedInfoObject.PickupPointPart.CityName,
                        Province = inflatedInfoObject.PickupPointPart.ProvinceName,
                        PostalCode = inflatedInfoObject.PickupPointPart.PostalCode,
                        Country = inflatedInfoObject.PickupPointPart.CountryName
                    };
                }
            }
        }

        public override void ReinflateViewModel(CheckoutViewModel viewModel) {
            if (viewModel.ProviderViewModels.ContainsKey(ProviderId)) {
                var providerVm = viewModel.ProviderViewModels[ProviderId];
                if (providerVm != null && providerVm is JObject) {
                    // "cast" to an object of the right class
                    viewModel.ProviderViewModels[ProviderId] = 
                        (providerVm as JObject).ToObject<PickupPointsCheckoutViewModel>();
                }
            }
        }

        public override IEnumerable<dynamic> GetOrderShippingAddressShapes(OrderPart orderPart) {
            var pickupPointOrderPart = orderPart.As<PickupPointOrderPart>();
            var addressIsSame = false;
            if (pickupPointOrderPart.IsOrderPickupPoint) {
                // compare address in part with the one currently in the order
                var addressPart = pickupPointOrderPart.As<AddressOrderPart>();
                // if anything is now different, flag it
                if (addressPart != null) {
                    addressIsSame =
                        pickupPointOrderPart.CountryName.Equals(addressPart.ShippingCountryName)
                        && pickupPointOrderPart.CountryId == addressPart.CountryId
                        && pickupPointOrderPart.ProvinceName.Equals(addressPart.ShippingProvinceName)
                        && pickupPointOrderPart.ProvinceId == addressPart.ProvinceId
                        && pickupPointOrderPart.CityName.Equals(addressPart.ShippingCityName)
                        && pickupPointOrderPart.CityId == addressPart.CityId
                        ;
                }
                // street address and postal code is not stored in AddressOrderPart
                if (addressIsSame) {
                    if (orderPart != null) {
                        var shippingAddress = orderPart.ShippingAddress;
                        // these strings may be null in both addresses
                        addressIsSame &=
                            string.Equals(pickupPointOrderPart.AddressLine1, shippingAddress.Address1)
                            && string.Equals(pickupPointOrderPart.AddressLine2, shippingAddress.Address2)
                            && string.Equals(pickupPointOrderPart.PostalCode, shippingAddress.PostalCode);
                    }
                }
            }
            yield return _shapeFactory.PickupPointOrderShippingAddress(
                OrderPart: orderPart,
                PickupPointOrderPart: pickupPointOrderPart,
                AddressHasChanged: !addressIsSame
                );
        }

        private PickupPointsCheckoutViewModel InflatePickupPointsVM(CheckoutViewModel cvm) {
            // get existing view model for pickup points (if any)
            var viewModel = cvm.ProviderViewModels.ContainsKey(ProviderId)
                ? (PickupPointsCheckoutViewModel)cvm.ProviderViewModels[ProviderId]
                : new PickupPointsCheckoutViewModel();

            if (viewModel.SelectedPickupPointId > 0) {
                var part = _contentManager
                    .Get<PickupPointPart>(viewModel.SelectedPickupPointId);
                if (part != null) {
                    viewModel.PickupPointPart = part;
                }
            }

            return viewModel;
        }

        
    }
}