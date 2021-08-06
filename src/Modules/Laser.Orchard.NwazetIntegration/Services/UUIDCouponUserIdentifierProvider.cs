using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Nwazet.Commerce.Services.Couponing;
using Orchard.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class UUIDCouponUserIdentifierProvider : ICouponUserIdentifierProvider {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UUIDCouponUserIdentifierProvider(
            IHttpContextAccessor httpContextAccessor) {

            _httpContextAccessor = httpContextAccessor;
        }
        public int Priority => 10;

        public string GetAdditionalUserIdentifier(CouponApplicabilityContext context) {
            var UUIdentifier = GetRequestKey();
            if (!string.IsNullOrWhiteSpace(UUIdentifier)) {
                return UUIdentifier;
            }
            return null;
        }

        public string GetIdentifierType(CouponApplicabilityContext context) {
            return "Device UUID";
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