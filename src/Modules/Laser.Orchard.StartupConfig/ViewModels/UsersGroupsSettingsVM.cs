using Orchard.Environment.Extensions;
using System.Collections.Generic;
namespace Laser.Orchard.StartupConfig.ViewModels {
    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class UsersGroupsSettingsVM {
        //   public virtual SettingsRecord Settings { get; set; }
        public virtual List<ExtendedUsersGroupsRecordVM> ExtendedUsersGroupsListVM { get; set; }
    }
    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class ExtendedUsersGroupsRecordVM {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public bool Delete { get; set; }
    }
}
//using Laser.Orchard.StartupConfig.Models;
//using Orchard.Environment.Extensions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

//namespace Laser.Orchard.StartupConfig.ViewModels {
//    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
//    public class UsersGroupsSettingsVM {
//        public readonly UsersGroupsSettings ThePart { get; set; }

//        //private readonly UsersGroupsSettingsPart _part;

//        //public UsersGroupsSettingsVM(UsersGroupsSettingsPart part) {
//        //    _part = part;
//        //    //   this.ListGroup = _part.TypePartDefinition.Settings.GetModel<UsersGroupsSettingsPart>().GroupSerialized;
//        //    this.ListGroup = _part.GroupSerialized;
//        //}

//        //public List<Groupdata> ListGroup { get; set; }
//    }
//}