using Laser.Orchard.AdvancedSettings.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.AdvancedSettings.Handlers {
    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeSkinsSettingsPartHandler : ContentHandler {

        public ThemeSkinsSettingsPartHandler() {

            T = NullLocalizer.Instance;

            Filters.Add(new ActivatingFilter<ThemeSkinsSettingsPart>("Site"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType.Equals("Site")) {
                context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("ThemeSkins")));
            }
        }

    }
}