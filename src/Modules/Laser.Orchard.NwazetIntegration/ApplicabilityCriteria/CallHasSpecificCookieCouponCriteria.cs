using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.Localization;
using Orchard.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    public class CallHasSpecificCookieCouponCriteria
        : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CallHasSpecificCookieCouponCriteria(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals,
            IHttpContextAccessor httpContextAccessor)
            : base(workContextAccessor, cacheManager, signals) {

            _httpContextAccessor = httpContextAccessor;
            
        }
        
        public override string ProviderName => "CallHasSpecificCookieCouponCriteria";

        public override LocalizedString ProviderDisplayName => T("Criteria on the request cookies");

        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            describe
                .For("Request", T("Http Request"), T("Http Request"))
                .Element("Test request cookies",
                    T("Test request cookies"),
                    T("Test request cookies"),
                    (ctx) => ApplyCriteria(ctx),
                    (ctx) => ApplyCriteria(ctx),
                    (ctx) => CookieValueForm.DisplayFilter(ctx.State, T),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    CookieValueForm.FormName);
        }

        private void ApplyCriteria(CouponApplicabilityCriterionContext context) {

            if (context.IsApplicable) {
                var result = CookieValueForm.GetFilterPredicate(context.State)(_httpContextAccessor.Current().Request);

                if (!result) {
                    context.ApplicabilityContext.Message = 
                        T("Coupon code {0} is not valid", context.CouponRecord.Code);
                }
                context.IsApplicable = result;
                context.ApplicabilityContext.IsApplicable = result;
            }
        }
        
    }
}