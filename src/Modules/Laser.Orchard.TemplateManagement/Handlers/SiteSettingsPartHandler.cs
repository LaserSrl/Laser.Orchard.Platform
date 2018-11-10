using Laser.Orchard.TemplateManagement.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.TemplateManagement.Handlers {
    [OrchardFeature("Laser.Orchard.TemplateManagement")]
    public class SiteSettingsPartHandler : ContentHandler {
        public SiteSettingsPartHandler(IRepository<SiteSettingsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<SiteSettingsPart>("Site"));
            T = NullLocalizer.Instance;
            OnGetContentItemMetadata<SiteSettingsPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Template"))));
        }

        public Localizer T { get; set; }
    }
}