using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services.CheckoutShippingAddressProviders;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointsShippingCriteria : IApplicabilityCriterionProvider {

        private readonly IContentManager _contentManager;

        public PickupPointsShippingCriteria(
            IContentManager contentManager) {

            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeCriterionContext describe) {
            describe
                .For("Destination")
                .Element(
                    "Destination address isn't a pickup point",
                    T("Destination address isn't a pickup point"),
                    T("The destination address for this order isn't a pickup point"),
                    ApplyNotAPickupPointCriterion,
                    DisplayNotAPickupPointCriterion) // This criterion doesn't need a form
                .Element(
                    "Destination address is a pickup point",
                    T("Destination address is a pickup point"),
                    T("The destination address for this order is a pickup point"),
                    ApplyPickupPointCriterion,
                    DisplayPickupPointCriterion,
                    PickupPointsForm.FormName
                    )
                    ;
        }

        public void ApplyNotAPickupPointCriterion(CriterionContext context) {
            // Only test this criterion if we haven't already excluded the method
            if (context.IsApplicable) {
                var shippingComputeContext = context.ApplicabilityContext.ShippingContext;
                // We need the extended properties because we are going to work with information
                // from the shipping address providers.
                if (shippingComputeContext is ExtendedShippingOptionComputeContext) {
                    var extendedShippingContext = shippingComputeContext as ExtendedShippingOptionComputeContext;
                    // Make sure the user is selecting a pickup point
                    if (string.Equals(
                        PickupPointsCheckoutShippingAddressProvider.ProviderId, 
                        extendedShippingContext.ShippingAddressProviderId)) {
                        // If this is the case, then we are done:
                        context.IsApplicable = false;
                    }
                }
            }
        }

        public void ApplyPickupPointCriterion(CriterionContext context) {
            // Only test this criterion if we haven't already excluded the method
            if (context.IsApplicable) {
                // Assume that we'll find that the destination doesn't match what's configured for
                // the criterion. Setting this here also causes the criterion to return false when
                // invoked with a context that isn't ExtendedShippingOptionComputeContext.
                var applicable = false;

                var shippingComputeContext = context.ApplicabilityContext.ShippingContext;
                // We need the extended properties because we are going to work with information
                // from the shipping address providers.
                if (shippingComputeContext is ExtendedShippingOptionComputeContext) {
                    var extendedShippingContext = shippingComputeContext as ExtendedShippingOptionComputeContext;
                    // Make sure the user is selecting a pickup point
                    if (string.Equals(
                        PickupPointsCheckoutShippingAddressProvider.ProviderId,
                        extendedShippingContext.ShippingAddressProviderId)) {
                        // Parse the configured state for the criterion to see what pickup point
                        // we should be considering.
                        var pointIdsString = (string)context.State.PickupPointIds;
                        var idStrings = pointIdsString.Split(',');
                        // The empty string is the "any" selector for pickup points: if that was
                        // selected, we don't have to do anything else here, because any pickup
                        // point is fine.
                        if (idStrings.Any(s => s.Length == 0)) {
                            applicable = true;
                        } else {
                            // Here: "Any" was not selected, so we need to actually check what pickup
                            // point the user has selected, and what was configured for the criterion.
                            var providerViewModel =
                                (PickupPointsCheckoutViewModel)extendedShippingContext.ShippingAddressProviderViewModel;
                            if (providerViewModel != null) {
                                var selectedPickupPointId = providerViewModel.SelectedPickupPointId;
                                var configuredIds = idStrings
                                    .Select(s => {
                                        int i = 0;
                                        if (!int.TryParse(s, out i)) {
                                            i = -1;
                                        }
                                        return i;
                                    }).Where(i => i > 0);
                                // If the selected id is configured, the criterion is valid
                                applicable = configuredIds.Any(i => i == selectedPickupPointId);
                            } else {
                                // TODO: could that view model ever be null?
                            }
                        }
                    }
                }

                context.IsApplicable &= applicable;
            }
        }

        public LocalizedString DisplayNotAPickupPointCriterion(CriterionContext context) {
            return T("The destination address for this order isn't a pickup point");
        }

        public LocalizedString DisplayPickupPointCriterion(CriterionContext context) {
            var pointIdsString = (string)context.State.PickupPointIds;
            var idStrings = pointIdsString.Split(',');
            // The empty string is the "any" selector for pickup points
            if (idStrings.Any(s => s.Length == 0)) {
                return T("The destination address for this order is any pickup point");
            }
            // Given the selected points, get their display texts, and join them in a
            // single comma separated string to display out.
            var ids = idStrings
                .Select(s => {
                    int i = 0;
                    if (!int.TryParse(s, out i)) {
                        i = -1;
                    }
                    return i;
                }).Where(i => i > 0);
            var contents = _contentManager
                .GetMany<PickupPointPart>(ids, VersionOptions.Published, QueryHints.Empty);
            var texts = contents
                .Select(_contentManager.GetItemMetadata)
                .Select(x => x.DisplayText).ToArray();
            var displayText = string.Join(", ", texts);
            return T("The destination address for this order is among these pickup points: {0}", displayText);
        }
    }
}