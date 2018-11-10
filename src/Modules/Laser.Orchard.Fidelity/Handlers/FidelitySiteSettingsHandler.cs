using Laser.Orchard.Fidelity.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.Fidelity.Handlers
{
    public class FidelitySiteSettingsHandler : ContentHandler
    {
        public Localizer T { get; set; }

        public FidelitySiteSettingsHandler()
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<FidelitySiteSettingsPart>("Site"));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;

            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Fidelity")));
        }
    }
}