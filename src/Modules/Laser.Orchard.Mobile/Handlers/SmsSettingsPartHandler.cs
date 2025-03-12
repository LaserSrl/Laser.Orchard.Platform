using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.Mobile.Handlers {

    [OrchardFeature("Laser.Orchard.Sms")]
    public class SmsSettingsPartHandler : ContentHandler {

        public SmsSettingsPartHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<SmsSettingsPart>("Site"));
            //Filters.Add(new TemplateFilterForPart<SmsSettingsPart>("SmsSettingsPart_Edit", "Parts/SmsSettingsPart_Edit", "SMS"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("SMS")));
        }
    }
}