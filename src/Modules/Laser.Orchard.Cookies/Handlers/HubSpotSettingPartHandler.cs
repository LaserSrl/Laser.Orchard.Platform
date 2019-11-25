using Laser.Orchard.Cookies.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;

namespace Laser.Orchard.Cookies.Handlers {
    public class HubSpotSettingPartHandler : ContentHandler {
        public HubSpotSettingPartHandler()
        {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Filters.Add(new ActivatingFilter<HubSpotSettingsPart>("Site"));
        }

        public Localizer T { get; set; }
        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
            {
                return;
            }
            base.GetItemMetadata(context);
            var groupInfo = new GroupInfo(T("HubSpot"));
            groupInfo.Id = "HubSpot";
            context.Metadata.EditorGroupInfo.Add(groupInfo);
        }

    }
}