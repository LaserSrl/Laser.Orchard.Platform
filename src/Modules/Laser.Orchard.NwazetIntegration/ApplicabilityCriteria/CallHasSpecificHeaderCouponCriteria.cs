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
    public class CallHasSpecificHeaderCouponCriteria
        : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CallHasSpecificHeaderCouponCriteria (
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals,
            IHttpContextAccessor httpContextAccessor)
            : base(workContextAccessor, cacheManager, signals) {

            _httpContextAccessor = httpContextAccessor;
            
        }
        
        public override string ProviderName => "CallHasSpecificHeaderCouponCriteria";

        public override LocalizedString ProviderDisplayName => T("Criteria on the request header");

        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            describe
                .For("Request", T("Request header"), T("Request header"))
                .Element("Test request header",
                    T("Test request header"),
                    T("Test request header"),
                    (ctx) => ApplyCriteria(ctx),
                    (ctx) => ApplyCriteria(ctx),
                    (ctx) => HeaderValueForm.DisplayFilter(ctx.State, T),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    HeaderValueForm.FormName);
        }

        private void ApplyCriteria(CouponApplicabilityCriterionContext context) {

            if (context.IsApplicable) {
                var result = HeaderValueForm.GetFilterPredicate(context.State)(_httpContextAccessor.Current().Request);

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