using Laser.Orchard.Cookies;
using Orchard.ContentManagement;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class EcommerceAnalyticsSettingsPart : ContentPart {
        // cookie level
        public CookieType EcommerceCookieLevel {
            get { return this.Retrieve(x => x.EcommerceCookieLevel); }
            set { this.Store(x => x.EcommerceCookieLevel, value); }
        }
    }
}