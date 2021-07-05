using Laser.Orchard.StartupConfig.TaxonomiesExtensions.Projections;
using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    public class CartHasProductWithTermCouponLineCriteria
        : BaseCouponCriterionProvider, ICouponLineApplicabilityCriterionProvider {
        private readonly ITaxonomyService _taxonomyService;

        public CartHasProductWithTermCouponLineCriteria(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals,
            ITaxonomyService taxonomyService)
            : base(workContextAccessor, cacheManager, signals) {

            _taxonomyService = taxonomyService;
        }

        public override string ProviderName => "CartHasProductWithTermCouponLineCriteria";

        public override LocalizedString ProviderDisplayName => T("Line Criteria on product TaxonomyFields");

        public void Describe(DescribeCouponLineApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            describe
               .For("Cart", T("Cart products"), T("Cart products"))
               // Product in line MUST have selected term
               .Element("Line product is in category",
                    T("Line product is in the given category (if any was selected)"),
                    T("Line product is in the given category (if any was selected)"),
                    (ctx) => ApplyCriterion(ctx, (b) => b),
                    (ctx) => DisplayTrueLabel(ctx),
                    (ctx) => T("Coupon {0} is not valid for any product in your cart.", ctx.Coupon.Code),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    SelectTermsForm.FormName)
               // Product in line MUST not have selected term
               .Element("Line product is not in category",
                    T("Line product is not in the given category (if any was selected)"),
                    T("Line product is not in the given category (if any was selected)"),
                    (ctx) => ApplyCriterion(ctx, (b) => !b),
                    (ctx) => DisplayFalseLabel(ctx),
                    (ctx) => T("Coupon {0} is not valid for any product in your cart.", ctx.Coupon.Code),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    SelectTermsForm.FormName);
        }

        public LocalizedString DisplayFalseLabel(CouponContext ctx) {
            bool.TryParse(ctx.State.IncludeChildren?.Value, out bool includeChildren);
            if (includeChildren) {
                return T("Cart line is not a product of the given category or one of its children.");
            }
            return T("Cart line is not a product of the given category.");
        }

        public LocalizedString DisplayTrueLabel(CouponContext ctx) {
            bool.TryParse(ctx.State.IncludeChildren?.Value, out bool includeChildren);
            if (includeChildren) {
                return T("Cart line is a product of the given category or one of its children.");
            }
            return T("Cart line is a product of the given category.");
        }

        public void ApplyCriterion(CouponLineCriterionContext context,
            // Use outerCriterion to negate the test, so we can easily do
            // contains / doesn't contain
            Func<bool, bool> outerCriterion) {

            if (context.IsApplicable) {
                // If there is no configured term, this delegate will not affect the
                // applicability for the coupon in any way.
                // get from state the comma separated list of term ids
                var termsString = (string)context.State.Terms;
                if (!string.IsNullOrWhiteSpace(termsString)) {

                    var product = context.ApplicabilityContext
                        .CartLine
                        ?.Product;
                    var termsPart = product.As<TermsPart>();

                    var termIds = termsString
                        .Split(new char[] { ',' })
                        .Select(int.Parse);
                    if (termIds.Any()) {
                        bool.TryParse(context.State.IncludeChildren?.Value, out bool includeChildren);
                        var terms = termIds
                            .SelectMany(id => GetTerms(id, includeChildren))
                            .Distinct();
                        // check again, in case the ids do not match any term
                        if (terms.Any()) {
                            var result = false;
                            // if no term is selected in the product, we already know this will be false
                            if (termsPart != null) {
                                var selectedTerms = termsPart.TermParts.Select(tcip => tcip.TermPart);
                                if (selectedTerms.Any()) {
                                    int op = Convert.ToInt32(context.State.Operator);

                                    switch (op) {
                                        case 0:
                                        // is one of
                                        result = terms.Any(t => selectedTerms.Contains(t));
                                        break;
                                        case 1:
                                        // is all of
                                        result = terms.All(t => selectedTerms.Contains(t));
                                        break;
                                    }
                                }
                            }

                            result = outerCriterion(result);

                            if (!result) {
                                var productName = "";
                                var contentManager = product?.ContentItem?.ContentManager;
                                if (contentManager != null) {
                                    productName = contentManager.GetItemMetadata(product).DisplayText;
                                }
                                if (!string.IsNullOrWhiteSpace(productName)) {
                                    context.ApplicabilityContext.Message =
                                        T("Coupon {0} is not valid for {1}.",
                                            context.CouponRecord.Code,
                                            productName);
                                }
                            }

                            context.ApplicabilityContext.IsApplicable = result;
                            context.IsApplicable = result;
                        }
                    }
                }
            }
        }

        private IEnumerable<TermPart> GetTerms(int termId, bool andChildren) {
            var term = _taxonomyService.GetTerm(termId);
            if (term == null) {
                return Enumerable.Empty<TermPart>();
            }
            var ts = new List<TermPart>() { term };
            if (andChildren) {
                ts.AddRange(_taxonomyService.GetChildren(term));
            }
            return ts;
        }
    }
}