using Laser.Orchard.WebServices.Models;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.WebServices.Handlers {
    public class WebServiceSettingsHandler: ContentHandler
    {
        public WebServiceSettingsHandler()
        {
            Filters.Add(new ActivatingFilter<WebServiceSettingsPart>("Site"));
        }

        //protected override void GetItemMetadata(GetContentItemMetadataContext context)
        //{
        //    if (context.ContentItem.ContentType != "Site")
        //        return;

        //    base.GetItemMetadata(context);
        //}
    }
}

