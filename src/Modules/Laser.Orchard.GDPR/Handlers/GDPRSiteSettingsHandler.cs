using Laser.Orchard.GDPR.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;

namespace Laser.Orchard.GDPR.Handlers {
    public class GDPRSiteSettingsHandler: ContentHandler {
        public GDPRSiteSettingsHandler() {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Filters.Add(new ActivatingFilter<GDPRSiteSettingsPart>("Site"));
        }

        public Localizer T { get; set; }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site") {
                return;
            }
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("GDPR")));
        }
    }
}