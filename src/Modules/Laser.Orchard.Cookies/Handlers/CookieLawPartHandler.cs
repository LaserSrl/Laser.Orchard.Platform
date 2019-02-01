using Laser.Orchard.Cookies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.Cookies.Handlers
{
    public class CookieLawPartHandler : ContentHandler {
        public CookieLawPartHandler()
        {
            T = NullLocalizer.Instance;
            OnInitializing<CookieLawPart>((context, part) => {
                part.cookieMessage = T("We use cookies on this website. For more infos please visit the following link.").Text;
                part.cookieAcceptButtonText = T("OK").Text;
                part.cookieResetButtonText = T("GDPR Cookie").Text;
                part.cookieTitle = T("This website uses cookies").Text;
                part.cookiePolicyPageMessage = T("More infos").Text;
            });
        }

        public Localizer T { get; set; }
    }
}