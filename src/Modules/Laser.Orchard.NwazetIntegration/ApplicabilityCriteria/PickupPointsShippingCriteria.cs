using Laser.Orchard.NwazetIntegration.Services.CheckoutShippingAddressProviders;
using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Nwazet.Commerce.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointsShippingCriteria : IApplicabilityCriterionProvider {

        public PickupPointsShippingCriteria() {

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeCriterionContext describe) {
            describe
                .For("Destination")
                .Element(
                    "Shipping address isn't a pickup point",
                    T("Shipping address isn't a pickup point"),
                    T("The shipping address for this order isn't a pickup point"),
                    ApplyNotAPickupPointCriterion,
                    DisplayNotAPickupPointCriterion) // This criterion doesn't need a form
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

        public LocalizedString DisplayNotAPickupPointCriterion(CriterionContext context) {
            return T("The shipping address for this order isn't a pickup point");
        }

        public void ApplyCriteria(CriterionContext context) {

        }

        public LocalizedString DisplayCriteria(CriterionContext context) {
            return T("");
        }
    }
}