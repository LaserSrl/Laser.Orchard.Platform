using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    public class CartHasProductDiscountCouponLineCriteria
        : BaseCouponCriterionProvider, ICouponLineApplicabilityCriterionProvider {

        public CartHasProductDiscountCouponLineCriteria(
           IWorkContextAccessor workContextAccessor,
           ICacheManager cacheManager,
           ISignals signals)
           : base(workContextAccessor, cacheManager, signals) {
        }

        public override string ProviderName => "CartHasProductDiscountCouponLineCriteria";

        public override LocalizedString ProviderDisplayName => T("Line Criteria on product not discounted");

        public void Describe(DescribeCouponLineApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            describe
               .For("Cart", T("Cart products"), T("Cart products"))
               .Element("Line product should not be discounted",
                    T("Line product should not be discounted"),
                    T("Line product should not be discounted"),
                    (ctx) => ApplyCriterion(ctx),
                    (ctx) => T("Line product should not be discounted."),
                    (ctx) => T("Coupon {0} is not valid for any product in your cart.", ctx.Coupon.Code),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    null);
        }

        public void ApplyCriterion(CouponLineCriterionContext context) {
            var result = false;
            if (context.IsApplicable) {
                // get product in shopping cart
                var product = context
                  .ApplicabilityContext
                  .CartLine
                  ?.Product;

                if (product != null) {
                    // to apply the coupon, the product must be discounted
                    result = !HasDiscount(product);
                }
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