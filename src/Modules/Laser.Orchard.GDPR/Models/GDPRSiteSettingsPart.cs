using Orchard.ContentManagement;

namespace Laser.Orchard.GDPR.Models {
    public class GDPRSiteSettingsPart : ContentPart {
        public bool DisableCookieManagement {
            get {
                return this.Retrieve(x => x.DisableCookieManagement);
            }
            set { this.Store(x => x.DisableCookieManagement, value); }
        }
    }
}