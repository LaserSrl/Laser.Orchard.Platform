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