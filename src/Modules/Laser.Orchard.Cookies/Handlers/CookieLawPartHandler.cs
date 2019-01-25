using Laser.Orchard.Cookies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.Cookies.Handlers
{
    public class CookieLawPartHandler : ContentHandler {

        private const string cookiemsg = "We use cookies on this website, you can <a href=\"{{cookiePolicyLink}}\" title=\"read about our cookies\">read about them here</a>. To use the website as intended please...";
        private const string cookieanalyticsmsg = "We use cookies, just to track visits to our website, we store no personal details. To use the website as intended please...";
        private const string acceptmsg = "ACCEPT COOKIES";
        private const string declinemsg = "DECLINE COOKIES";
        private const string resetmsg = "RESET COOKIES FOR THIS WEBSITE";
        private const string whataremsg = "What are Cookies?";
        private const string discreetmsg = "Cookies?";
        private const string errormsg = "We're sorry, you declined the use of cookies on this website, this feature places cookies in your browser and has therefore been disabled.<br>To continue this functionality please";
        private const string whatarecookieslink = "http://www.allaboutcookies.org/";

        public CookieLawPartHandler()
        {
            T = NullLocalizer.Instance;

            OnInitializing<CookieLawPart>((context, part) => {
                part.cookiePolicyLink = string.Empty;
                part.cookieMessage = cookiemsg;
                part.cookieWhatAreTheyLink = whatarecookieslink;
                part.cookieErrorMessage = errormsg;
                part.cookieAcceptButtonText = acceptmsg;
                part.cookieResetButtonText = resetmsg;
                part.cookieWhatAreLinkText = whataremsg;
                part.cookiePolicyPageMessage = string.Empty;
                part.cookieDiscreetLinkText = string.Empty;
            });
        }

        public Localizer T { get; set; }

    }
}