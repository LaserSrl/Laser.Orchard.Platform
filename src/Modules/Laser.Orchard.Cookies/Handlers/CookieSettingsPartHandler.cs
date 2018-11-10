using Laser.Orchard.Cookies.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.Cookies.Handlers
{
    public class CookieSettingsPartHandler : ContentHandler
    {
        //public CookieSettingsPartHandler(IRepository<CookieSettingsPartRecord> repository)
        //{
        //    Filters.Add(StorageFilter.For(repository));

        public CookieSettingsPartHandler()
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<CookieSettingsPart>("Site"));

            //OnInitializing<CookieSettingsPart>((context, part) =>
            //{
            //        part.cookieNotificationLocationBottom = false;           
            //        part.cookieAnalytics = true;                                         
            //        part.showCookieDeclineButton = false;              
            //        part.showCookieAcceptButton = true;               
            //        part.showCookieResetButton = false;                
            //        part.cookieOverlayEnabled = false;                         
            //        part.cookieCutter = false;           
            //        part.cookieDisable = string.Empty;                        
            //        part.cookiePolicyPage = false;                             
            //        part.cookieDiscreetLink = false;                           
            //        part.cookieDiscreetPosition = "topleft";            
            //        part.cookieDomain = string.Empty;
            //        part.cookieDiscreetReset = false;
            //});
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Cookies")));
        }
    }
}