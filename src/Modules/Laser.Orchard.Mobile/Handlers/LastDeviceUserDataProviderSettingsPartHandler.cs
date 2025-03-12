using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.Mobile.Handlers {
    public class LastDeviceUserDataProviderSettingsPartHandler : ContentHandler {
        public LastDeviceUserDataProviderSettingsPartHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<LastDeviceUserDataProviderSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<LastDeviceUserDataProviderSettingsPart>("LastDeviceUserDataProviderSettings", "Parts/Users.LastDeviceUserDataProviderSettings", "users"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Users")));
        }
    }
}