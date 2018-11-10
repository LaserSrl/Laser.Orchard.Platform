using Laser.Orchard.Cookies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.Cookies.Handlers
{
    public class CookieLawPartHandler : ContentHandler {
        //public CookieLawPartHandler(IRepository<CookieLawPartRecord> repository) {
            
        //    Filters.Add(StorageFilter.For(repository));

        public CookieLawPartHandler()
        {
            T = NullLocalizer.Instance;

            OnInitializing<CookieLawPart>((context, part) => {
                part.cookieAnalyticsMessage = Migrations.cookieanalyticsmsg;
                part.cookiePolicyLink = string.Empty;
                part.cookieMessage = Migrations.cookiemsg;
                part.cookieWhatAreTheyLink = Migrations.whatarecookieslink;
                part.cookieErrorMessage = Migrations.errormsg;
                part.cookieAcceptButtonText = Migrations.acceptmsg;
                part.cookieDeclineButtonText = Migrations.declinemsg;
                part.cookieResetButtonText = Migrations.resetmsg;
                part.cookieWhatAreLinkText = Migrations.whataremsg;
                part.cookiePolicyPageMessage = string.Empty;
                part.cookieDiscreetLinkText = string.Empty;
            });
        }

        public Localizer T { get; set; }

    }
}