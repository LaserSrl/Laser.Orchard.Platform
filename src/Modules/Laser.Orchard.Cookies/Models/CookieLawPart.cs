using Orchard.ContentManagement;

namespace Laser.Orchard.Cookies.Models
{

    public class CookieLawPart : ContentPart
    {
        public string cookieDiscreetLinkText { get { return this.Retrieve(x => x.cookieDiscreetLinkText); } set { this.Store(x => x.cookieDiscreetLinkText, value); } }
        public string cookiePolicyPageMessage { get { return this.Retrieve(x => x.cookiePolicyPageMessage); } set { this.Store(x => x.cookiePolicyPageMessage, value); } }
        public string cookieErrorMessage { get { return this.Retrieve(x => x.cookieErrorMessage); } set { this.Store(x => x.cookieErrorMessage, value); } }
        public string cookieAcceptButtonText { get { return this.Retrieve(x => x.cookieAcceptButtonText); } set { this.Store(x => x.cookieAcceptButtonText, value); } }
        public string cookieDeclineButtonText { get { return this.Retrieve(x => x.cookieDeclineButtonText); } set { this.Store(x => x.cookieDeclineButtonText, value); } }
        public string cookieResetButtonText { get { return this.Retrieve(x => x.cookieResetButtonText); } set { this.Store(x => x.cookieResetButtonText, value); } }
        public string cookieWhatAreLinkText { get { return this.Retrieve(x => x.cookieWhatAreLinkText); } set { this.Store(x => x.cookieWhatAreLinkText, value); } }
        public string cookieAnalyticsMessage { get { return this.Retrieve(x => x.cookieAnalyticsMessage); } set { this.Store(x => x.cookieAnalyticsMessage, value); } }
        public string cookiePolicyLink { get { return this.Retrieve(x => x.cookiePolicyLink); } set { this.Store(x => x.cookiePolicyLink, value); } }
        public string cookieMessage { get { return this.Retrieve(x => x.cookieMessage); } set { this.Store(x => x.cookieMessage, value); } }
        public string cookieWhatAreTheyLink { get { return this.Retrieve(x => x.cookieWhatAreTheyLink); } set { this.Store(x => x.cookieWhatAreTheyLink, value); } }
    }
}