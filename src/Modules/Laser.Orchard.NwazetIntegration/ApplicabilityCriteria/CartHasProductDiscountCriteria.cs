using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.Localization;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    public class CartHasProductDiscountCriteria
         : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {
        public CartHasProductDiscountCriteria(
          IWorkContextAccessor workContextAccessor,
          ICacheManager cacheManager,
          ISignals signals)
          : base(workContextAccessor, cacheManager, signals) {

        }
        public override string ProviderName => "CartHasProductDiscountCriteria";
        public override LocalizedString ProviderDisplayName => T("Coupon is applicable only if there is no discounted product in the cart.");
        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();
            describe
                .For("Cart", T("Cart products"), T("Cart products"))
                .Element("Line products shuld not be discounted",
                    T("Line products should not be discounted"),
                    T("If there is a discounted product in the cart, the coupon cannot be applied."),
                    (ctx) => ApplyCriteria(ctx),
                    (ctx) => ApplyCriteria(ctx),
                    (ctx) => T("Coupon is applicable only if there is no discounted product in the cart."),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    null);
        }
        public void ApplyCriteria(CouponApplicabilityCriterionContext context) {
            var result = false;
            if (context.IsApplicable) {
                // get product in shopping cart
                var products = context
                  .ApplicabilityContext
                  .ShoppingCart
                  .GetProducts();

                // to apply the coupon, no products in the cart must be discounted 
                result = !products.Any(p => HasDiscount(p.Product));

                context.ApplicabilityContext.IsApplicable = result;
                context.IsApplicable = result;
            }
        }

        // check if products are discounted
        public bool HasDiscount(ProductPart part) {
            var discountPrice = part.ProductPriceService.GetDiscountPrice(part);
            var fullPrice = part.ProductPriceService.GetPrice(part);
            var hasDiscount = discountPrice > 0M && discountPrice < fullPrice;
            return hasDiscount;
        }
    }
}
