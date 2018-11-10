using Laser.Orchard.Translator.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.Translator.Handlers
{
    public class TranslatorSettingsHandler : ContentHandler
    {
        public Localizer T { get; set; }

        public TranslatorSettingsHandler()
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<TranslatorSettingsPart>("Site"));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;

            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Translator")));
        }
    }
}