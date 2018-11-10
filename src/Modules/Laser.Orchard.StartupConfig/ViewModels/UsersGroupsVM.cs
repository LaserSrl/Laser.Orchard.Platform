using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ViewModels {
    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    [ValidateUserGroup]
    public class UsersGroupsVM {
        //       private readonly UsersGroupsPart _UsersGroups;
        public UsersGroupsVM() {//UsersGroupsPart UsersGroups){
            //           _UsersGroups=UsersGroups;
        }
        public List<ExtendedUsersGroupsRecordVM> ListOfGroups {
            get;
            set;
        }
        public List<int> GroupNumber { get; set; }
        public bool Required { get; set; }

            
        //public List<ExtendedUsersGroupsRecord> getSelectedValue {
        //    get {
        //       return (this.ListOfGroups.Where(s => this.GroupNumber.Contains(s.Id)).ToList());

        //    }
        //}
    }


}