using System;
using System.Collections.Generic;
using Laser.Orchard.GDPR.Extensions;

namespace Laser.Orchard.GDPR.Services {
    public class GDPRScript : IGDPRScript {
        public string GetBannerChoice() {
            throw new NotImplementedException();
        }

        public List<CookieType> GetCurrentGDPR() {
            throw new NotImplementedException();
        }

        public void SetCurrentGDPR(List<CookieType> cookieTypes) {
            throw new NotImplementedException();
        }
    }
}