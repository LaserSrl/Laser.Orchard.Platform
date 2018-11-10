//using Laser.Orchard.StartupConfig.Models;
//using Orchard.ContentManagement;
//using Orchard.ContentManagement.Handlers;
//using Orchard.Data;
//using Orchard.Environment.Extensions;
//using Orchard.Localization;
//namespace Laser.Orchard.StartupConfig.Handlers {
//    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
//    public class UsersGroupsSettingsHandler : ContentHandler {

//        public UsersGroupsSettingsHandler(IRepository<UsersGroupsSettingsPartRecord> repository) {
//            Filters.Add(StorageFilter.For(repository));
//            Filters.Add(new ActivatingFilter<UsersGroupsSettingsPart>("Site"));
//            T = NullLocalizer.Instance;
//            OnGetContentItemMetadata<UsersGroupsSettingsPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("UserGroups"))));
//        }

//        public Localizer T { get; set; }
//    }
//}


