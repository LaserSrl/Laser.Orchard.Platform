using Laser.Orchard.ShareLink.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;

namespace Laser.Orchard.ShareLink.Handlers {

    public class ShareLinkModuleSettingHandler : ContentHandler {
        public Localizer T { get; set; }

        public ShareLinkModuleSettingHandler(IRepository<ShareLinkModuleSettingPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<ShareLinkModuleSettingPart>("Site"));
            T = NullLocalizer.Instance;
            OnGetContentItemMetadata<ShareLinkModuleSettingPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("ShareLink"))));
        }
    }
}