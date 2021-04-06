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
    public class CartHasProductOfTypeCouponLineCriteria
        : BaseCouponCriterionProvider, ICouponLineApplicabilityCriterionProvider {
        
        public CartHasProductOfTypeCouponLineCriteria(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals) 
            : base (workContextAccessor, cacheManager, signals) {
            
        }
        

        public override string ProviderName => "CartHasProductOfTypeCouponLineCriteria";

        public override LocalizedString ProviderDisplayName => T("Line Criteria on product ContentType");

        public void Describe(DescribeCouponLineApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();
            describe
                .For("Cart", T("Cart products"), T("Cart products"))
                // Product in line MUST BE one of the selected types
                .Element("Line product is of type",
                    T("Line product is of a given type"),
                    T("Line product is of a given type"),
                    (ctx) => ApplyCriterion(ctx, (b) => b),
                    (ctx) => DisplayTrueLabel(ctx),
                    (ctx) => T("Coupon {0} is not valid for any product in your cart.", ctx.Coupon.Code),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    ProductContentTypeForm.FormName)
                // Product in line MUST NOT BE one of the selected types
                .Element("Line product is not of type",
                    T("Line product is not of a given type"),
                    T("Line product is not of a given type"),
                    (ctx) => ApplyCriterion(ctx, (b) => !b),
                    (ctx) => DisplayFalseLabel(ctx),
                    (ctx) => T("Coupon {0} is not valid for any product in your cart.", ctx.Coupon.Code),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    ProductContentTypeForm.FormName);
        }

        public LocalizedString DisplayFalseLabel(CouponContext ctx) {
            return T("Cart line is not a product of type {0}", ProductTypes(ctx));
        }

        public LocalizedString DisplayTrueLabel(CouponContext ctx) {
            return T("Cart line is a product of type {0}", ProductTypes(ctx));
        }
        
        private string ProductTypes(CouponContext context) =>
            (string)context.State.ContentTypes;

        public void ApplyCriterion(CouponLineCriterionContext context,
            // Use outerCriterion to negate the test, so we can easily do
            // contains / doesn't contain
            Func<bool, bool> outerCriterion) {

            if (context.IsApplicable) {
                var allowedTypes = ProductTypes(context)
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim());
                var result = false;

                var product = context.ApplicabilityContext
                    .CartLine
                    ?.Product;
                var typeForLine = product
                    ?.ContentItem
                    ?.ContentType
                    ?? string.Empty;
                result = !string.IsNullOrWhiteSpace(typeForLine)
                    && allowedTypes.Contains(typeForLine);

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