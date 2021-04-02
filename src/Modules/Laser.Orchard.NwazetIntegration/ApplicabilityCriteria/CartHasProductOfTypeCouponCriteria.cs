using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    public class CartHasProductOfTypeCouponCriteria
        : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {

        public CartHasProductOfTypeCouponCriteria(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals)
            : base(workContextAccessor, cacheManager, signals) {
            
        }
        
        public override string ProviderName => 
            "CartHasProductOfTypeCouponCriteria";

        public override LocalizedString ProviderDisplayName => 
            T("Criteria for cart containing product ContentType.");

        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            describe
                .For("Cart", T("Cart products"), T("Cart products"))
                .Element("Cart has Products of given type",
                    T("Cart has Products of given type"),
                    T("Cart has Products of given type"),
                    (ctx) => ApplyCriteria(ctx, (b) => b, 
                        T("Coupon {0} is only available if cart contains products of type {1}.", ctx.CouponRecord.Code, ProductTypes(ctx))),
                    (ctx) => ApplyCriteria(ctx, (b) => b,
                        T("Coupon {0} is only available if cart contains products of type {1}.", ctx.CouponRecord.Code, ProductTypes(ctx))),
                    (ctx) => DisplayTrueLabel(ctx),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    ProductContentTypeForm.FormName)
                .Element("Cart doesn't have Products of given type",
                    T("Cart doesn't have Products of given type"),
                    T("Cart doesn't have Products of given type"),
                    (ctx) => ApplyCriteria(ctx, (b) => !b,
                        T("Coupon {0} is only available if cart doesn't contain products of type {1}.", ctx.CouponRecord.Code, ProductTypes(ctx))),
                    (ctx) => ApplyCriteria(ctx, (b) => !b,
                        T("Coupon {0} is only available if cart doesn't contain products of type {1}.", ctx.CouponRecord.Code, ProductTypes(ctx))),
                    (ctx) => DisplayFalseLabel(ctx),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    ProductContentTypeForm.FormName);
        }

        public void ApplyCriteria(CouponApplicabilityCriterionContext context,
            // Use outerCriterion to negate the test, so we can easily do
            // true/false
            Func<bool, bool> outerCriterion,
            LocalizedString failureMessage) {

            if (context.IsApplicable) {
                var allowedTypes = ProductTypes(context)
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim());
                var result = outerCriterion(
                    // the "base" test is "contains products of type". If
                    // outerCriterion is b => !b that turns in 
                    // "doesn't contain products of type"
                    context.ApplicabilityContext
                        .ShoppingCart
                        .GetProducts()
                        .Any(pq => allowedTypes.Contains(pq.Product.TypeDefinition.Name)));

                if (!result) {
                    context.ApplicabilityContext.Message = failureMessage;
                }
                context.IsApplicable = result;
                context.ApplicabilityContext.IsApplicable = result;
            }
        }


        public LocalizedString DisplayFalseLabel(CouponContext ctx) {
            return T("Cart contains products of type {0}", ProductTypes(ctx));
        }

        public LocalizedString DisplayTrueLabel(CouponContext ctx) {
            return T("Cart contains products of type {0}", ProductTypes(ctx));
        }

        private string ProductTypes(CouponContext context) =>
            (string)context.State.ContentTypes;
    }
}