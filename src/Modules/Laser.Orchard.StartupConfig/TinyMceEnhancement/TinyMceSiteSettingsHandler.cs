using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;

namespace Laser.Orchard.StartupConfig.TinyMceEnhancement {
    [OrchardFeature("Laser.Orchard.StartupConfig.TinyMceEnhancement")]
    public class TinyMceSiteSettingsHandler : ContentHandler {
        public Localizer T { get; set; }
        public TinyMceSiteSettingsHandler() {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Filters.Add(new ActivatingFilter<TinyMceSiteSettingsPart>("Site"));
        }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site") {
                return;
            }
            base.GetItemMetadata(context);
            var groupInfo = new GroupInfo(T("TinyMce Enhancement"));
            groupInfo.Id = "TinyMce";
            context.Metadata.EditorGroupInfo.Add(groupInfo);
        }
    }
}