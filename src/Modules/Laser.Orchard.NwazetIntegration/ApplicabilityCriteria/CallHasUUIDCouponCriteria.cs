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
    public class CallHasUUIDCouponCriteria
        : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CallHasUUIDCouponCriteria (
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals,
            IHttpContextAccessor httpContextAccessor)
            : base(workContextAccessor, cacheManager, signals) {

            _httpContextAccessor = httpContextAccessor;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string ProviderName => "CallHasUUIDCouponCriteria";

        public override LocalizedString ProviderDisplayName => T("Criteria on the presence of device UUID");

        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            describe
                .For("Request", T("Request has UUID header"), T("Request has UUID header"))
                .Element("Request has UUID",
                    T("Request has UUID"),
                    T("Request has UUID"),
                    (ctx) => ApplyCriteria(ctx, (b) => b, T("Coupon {0} is only available for mobile users.", ctx.CouponRecord.Code)),
                    (ctx) => ApplyCriteria(ctx, (b) => b, T("Coupon {0} is only available for mobile users.", ctx.CouponRecord.Code)),
                    (ctx) => T("Request has UUID header"),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    null)
                .Element("Request doesn't have UUID",
                    T("Request doesn't have UUID"),
                    T("Request doesn't have UUID"),
                    (ctx) => ApplyCriteria(ctx, (b) => !b, T("Coupon {0} is not available for mobile users.", ctx.CouponRecord.Code)),
                    (ctx) => ApplyCriteria(ctx, (b) => !b, T("Coupon {0} is not available for mobile users.", ctx.CouponRecord.Code)),
                    (ctx) => T("Request doesn't have UUID header"),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    null);
        }

        private void ApplyCriteria(CouponApplicabilityCriterionContext context,
            // Use outerCriterion to negate the test, so we can easily do
            // true/false
            Func<bool, bool> outerCriterion,
            LocalizedString failureMessage) {

            if (context.IsApplicable) {
                var UUIdentifier = GetRequestKey("UUID");
                var result = outerCriterion(!string.IsNullOrWhiteSpace(UUIdentifier));
                if (!result) {
                    context.ApplicabilityContext.Message = failureMessage;
                }
                context.IsApplicable = result;
                context.ApplicabilityContext.IsApplicable = result;
            }
        }

        private string GetRequestKey(string name = "UUID") {
            var result = "";
            var req = _httpContextAccessor.Current().Request;
            result = req.Headers["x-" + name];
            if (string.IsNullOrWhiteSpace(result)) {
                result = req.QueryString[name];
            }
            return result;
        }
    }
}