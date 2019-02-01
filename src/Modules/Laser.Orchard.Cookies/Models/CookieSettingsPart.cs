using Orchard.ContentManagement;

namespace Laser.Orchard.Cookies.Models
{
    public class CookieSettingsPart : ContentPart
    {
        public CookieBannerPosition cookiePosition { get { return this.Retrieve(x => x.cookiePosition, CookieBannerPosition.Overlay); } set { this.Store(x => x.cookiePosition, value); } }
        public string cookieDomain { get { return this.Retrieve(x => x.cookieDomain); } set { this.Store(x => x.cookieDomain, value); } }
        public bool cookieDiscreetReset { get { return this.Retrieve(x => x.cookieDiscreetReset, true); } set { this.Store(x => x.cookieDiscreetReset, value); } }
        public bool cookiePolicyPage { get { return this.Retrieve(x => x.cookiePolicyPage); } set { this.Store(x => x.cookiePolicyPage, value); } }
        public string cookieDisable { get { return this.Retrieve(x => x.cookieDisable); } set { this.Store(x => x.cookieDisable, value); } }
        public bool showCookieResetButton { get { return this.Retrieve(x => x.showCookieResetButton); } set { this.Store(x => x.showCookieResetButton, value); } }
        public bool defaultValuePreferences {
            get {
                return this.Retrieve(x => x.defaultValuePreferences, true);
            }
            set { this.Store(x => x.defaultValuePreferences, value); }
        }
        public bool defaultValueStatistical {
            get {
                return this.Retrieve(x => x.defaultValueStatistical, true);
            }
            set { this.Store(x => x.defaultValueStatistical, value); }
        }
        public bool defaultValueMarketing {
            get {
                return this.Retrieve(x => x.defaultValueMarketing, true);
            }
            set { this.Store(x => x.defaultValueMarketing, value); }
        }
        public bool cookieCutter { get { return this.Retrieve(x => x.cookieCutter); } set { this.Store(x => x.cookieCutter, value); } }
        public bool DisableCookieGDPRManagement {
            get {
                return this.Retrieve(x => x.DisableCookieGDPRManagement);
            }
            set { this.Store(x => x.DisableCookieGDPRManagement, value); }
        }
    }
}