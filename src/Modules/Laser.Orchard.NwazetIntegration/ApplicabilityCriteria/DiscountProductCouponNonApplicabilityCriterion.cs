using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ApplicabilityCriteria.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class DiscountProductCouponNonApplicabilityCriterion
         : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {
        public DiscountProductCouponNonApplicabilityCriterion(
          IWorkContextAccessor workContextAccessor,
          ICacheManager cacheManager,
          ISignals signals)
          : base(workContextAccessor, cacheManager, signals) {

        }
        public override string ProviderName => "DiscountProductCouponNonApplicabilityCriterion";
        public override LocalizedString ProviderDisplayName => T("Discount product coupon non-applicability criterion");
        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();
            describe
                .For("Cart", T("Cart products"), T("Cart products"))
                .Element("Discount product coupon non-applicability criterion",
                    T("Discount product coupon non-applicability criterion"),
                    T("If there is a discounted product in the cart, the coupon cannot be applied."),
                    (ctx) => ApplyCriteria(ctx),
                    (ctx) => ApplyCriteria(ctx),
                    (ctx) => T("Discount product coupon non-applicability criterion"),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    null);
        }
        public void ApplyCriteria(CouponApplicabilityCriterionContext context) {
            // Use outerCriterion to negate the test, so we can easily do
            // true/false
            var result = false;
            if (context.IsApplicable) {
                // get product in shopping cart
                var products = context
                  .ApplicabilityContext
                  .ShoppingCart
                  .GetProducts();

                result = !products.Any(p => HasDiscount(p.Product));

                context.ApplicabilityContext.IsApplicable = result;
                context.IsApplicable = result;
            }
        }

        public bool HasDiscount(ProductPart part) {
            var discountPrice = part.ProductPriceService.GetDiscountPrice(part);
            var fullPrice = part.ProductPriceService.GetPrice(part);
            var hasDiscount = discountPrice > 0M && discountPrice < fullPrice;
            return hasDiscount;
        }
    }
}
