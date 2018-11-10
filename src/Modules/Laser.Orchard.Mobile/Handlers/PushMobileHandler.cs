using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.Mobile.Handlers {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class SiteSettingsPartHandler : ContentHandler {
        public SiteSettingsPartHandler(IRepository<PushMobileSettingsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<PushMobileSettingsPart>("Site"));
            T = NullLocalizer.Instance;
            OnGetContentItemMetadata<PushMobileSettingsPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("PushMobile"))));
        }

        public Localizer T { get; set; }
    }
}