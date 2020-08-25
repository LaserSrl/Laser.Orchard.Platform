using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Nwazet.Commerce.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {

    public class CartHasProductOfTypeCriteria : IApplicabilityCriterionProvider {

        public CartHasProductOfTypeCriteria() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeCriterionContext describe) {

            describe
                .For("Cart", T("Cart products"), T("Cart products"))
                .Element("Cart has Products of given type",
                    T("Cart has Products of given type"),
                    T("Cart has Products of given type"),
                    (ctx) => ApplyCriteria(ctx, (b) => b),
                    DisplayTrueCriteria,
                    ProductContentTypeForm.FormName)
                .Element("Cart doesn't have Products of given type",
                    T("Cart doesn't have Products of given type"),
                    T("Cart doesn't have Products of given type"),
                    (ctx) => ApplyCriteria(ctx, (b) => !b),
                    DisplayFalseCriteria,
                    ProductContentTypeForm.FormName);
        }

        private string ProductTypes(CriterionContext context) =>
            (string)context.State.ContentTypes;

        public void ApplyCriteria(CriterionContext context,
            // Use outerCriterion to negate the test, so we can easily do
            // contains / doesn't contain
            Func<bool, bool> outerCriterion) {

            if (context.IsApplicable) {
                var types = ProductTypes(context)
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim());
                context.IsApplicable = outerCriterion(
                    // the "base" test is "contains products of type". If
                    // outerCriterion is b => !b that turns in 
                    // "doesn't contain products of type"
                    context.ApplicabilityContext
                        .ProductQuantities
                        .Any(pq => types.Contains(pq.Product.TypeDefinition.Name)));
            }
        }

        public LocalizedString DisplayTrueCriteria(CriterionContext context) {
            return T("Cart contains products of type {0}", ProductTypes(context));
        }
        public LocalizedString DisplayFalseCriteria(CriterionContext context) {
            return T("Cart doesn't contain products of type {0}", ProductTypes(context));
        }
    }

}