using Laser.Orchard.StartupConfig.TaxonomiesExtensions.Projections;
using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    public class CartHasProductWithTermCouponApplicabilityCriterion
         : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {

        private readonly ITaxonomyService _taxonomyService;

        public CartHasProductWithTermCouponApplicabilityCriterion(
          IWorkContextAccessor workContextAccessor,
          ICacheManager cacheManager,
          ISignals signals,
          ITaxonomyService taxonomyService)
          : base(workContextAccessor, cacheManager, signals) {

            _taxonomyService = taxonomyService;
        }

        public override string ProviderName => "CartHasProductWithTermCouponApplicabilityCriterion";

        public override LocalizedString ProviderDisplayName =>
         T("Criteria on product TaxonomyFields.");


        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            describe
               .For("Cart", T("Cart products"), T("Cart products"))
               // Product MUST have selected term
               .Element("Cart has Products in the category",
                    T("Cart has Products of the given category (if any was selected)"),
                    T("Cart has Products of the given category (if any was selected)"),
                    (ctx) => ApplyCriterion(ctx, (b) => b, T("Coupon {0} is not valid for any product in your cart.", ctx.CouponRecord.Code)),
                    (ctx) => ApplyCriterion(ctx, (b) => b, T("Coupon {0} is not valid for any product in your cart.", ctx.CouponRecord.Code)),
                    (ctx) => DisplayTrueLabel(ctx),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    SelectTermsForm.FormName)
               // Product MUST not have selected term
               .Element("Cart has not Products in the category",
                    T("Cart has not Products of the given category (if any was selected)"),
                    T("Cart has not Products of the given category(if any was selected))"),
                    (ctx) => ApplyCriterion(ctx, (b) => !b, T("Coupon {0} is not valid for any product in your cart.", ctx.CouponRecord.Code)),
                    (ctx) => ApplyCriterion(ctx, (b) => !b, T("Coupon {0} is not valid for any product in your cart.", ctx.CouponRecord.Code)),
                    (ctx) => DisplayFalseLabel(ctx),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    SelectTermsForm.FormName);
        }
        public LocalizedString DisplayFalseLabel(CouponContext ctx) {
            bool.TryParse(ctx.State.IncludeChildren?.Value, out bool includeChildren);
            if (includeChildren) {
                return T("Cart has not a products of the given category or one of its children.");
            }
            return T("Cart has not a products of the given category.");
        }

        public LocalizedString DisplayTrueLabel(CouponContext ctx) {
            bool.TryParse(ctx.State.IncludeChildren?.Value, out bool includeChildren);
            if (includeChildren) {
                return T("Cart has a products of the given category or one of its children.");
            }
            return T("Cart has a products of the given category.");
        }

        public void ApplyCriterion(CouponApplicabilityCriterionContext context,
            // Use outerCriterion to negate the test, so we can easily do
            // contains / doesn't contain
            Func<bool, bool> outerCriterion,
            LocalizedString failureMessage) {

            if (context.IsApplicable) {
                // If there is no configured term, this delegate will not affect the
                // applicability for the coupon in any way.
                // get from state the comma separated list of term ids
                var termsString = (string)context.State.Terms;
                if (!string.IsNullOrWhiteSpace(termsString)) {
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

                            // get product in shopping cart
                            var products = context
                              .ApplicabilityContext
                              .ShoppingCart
                              .GetProducts();
                            var termsPart = products.Select(p => p.Product.ContentItem.As<TermPart>());

                            // if no term is selected in the product, we already know this will be false
                            if (termsPart.Any()) {
                                result = terms.Any(t => termsPart.Contains(t));
                            }

                            result = outerCriterion(result);

                            if (!result) {
                                context.ApplicabilityContext.Message = failureMessage;
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