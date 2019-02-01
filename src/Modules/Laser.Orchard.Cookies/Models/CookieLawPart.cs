using Orchard.ContentManagement;
using Orchard.Localization;

namespace Laser.Orchard.Cookies.Models {

    public class CookieLawPart : ContentPart
    {
        public string cookieDiscreetLinkText { get { return this.Retrieve(x => x.cookieDiscreetLinkText); } set { this.Store(x => x.cookieDiscreetLinkText, value); } }
        public string cookiePolicyPageMessage { get { return this.Retrieve(x => x.cookiePolicyPageMessage, "More infos"); } set { this.Store(x => x.cookiePolicyPageMessage, value); } }
        public string cookieErrorMessage { get { return this.Retrieve(x => x.cookieErrorMessage); } set { this.Store(x => x.cookieErrorMessage, value); } }
        public string cookieAcceptButtonText { get { return this.Retrieve(x => x.cookieAcceptButtonText, "OK"); } set { this.Store(x => x.cookieAcceptButtonText, value); } }
        public string cookieResetButtonText { get { return this.Retrieve(x => x.cookieResetButtonText, "GDPR Cookie"); } set { this.Store(x => x.cookieResetButtonText, value); } }
        public string cookieWhatAreLinkText { get { return this.Retrieve(x => x.cookieWhatAreLinkText); } set { this.Store(x => x.cookieWhatAreLinkText, value); } }
        public string cookiePolicyLink { get { return this.Retrieve(x => x.cookiePolicyLink); } set { this.Store(x => x.cookiePolicyLink, value); } }
        public string cookieTitle { get { return this.Retrieve(x => x.cookieTitle, "This website uses cookies"); } set { this.Store(x => x.cookieTitle, value); } }
        public string cookieMessage { get { return this.Retrieve(x => x.cookieMessage, "We use cookies on this website. For more infos please visit the following link."); } set { this.Store(x => x.cookieMessage, value); } }
        public string cookieWhatAreTheyLink { get { return this.Retrieve(x => x.cookieWhatAreTheyLink); } set { this.Store(x => x.cookieWhatAreTheyLink, value); } }
    }
}