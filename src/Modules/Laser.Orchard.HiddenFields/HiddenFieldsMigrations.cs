using Orchard.Data.Migration;
using Orchard.Roles.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Services;

namespace Laser.Orchard.HiddenFields {
    public class HiddenFieldsMigrations : DataMigrationImpl {
        private readonly IUtilsServices _utilsServices;
        private readonly IRoleService _roleService;

        public HiddenFieldsMigrations(IUtilsServices utilsServices,
            IRoleService roleService) {
            _utilsServices = utilsServices;
            _roleService = roleService;
        }

        public int Create() {

            // To set the MaySeeHiddenFields permission for all default roles, we are now
            // correctly using stereotypes.

            ////update permissions for all existing roles:
            ////We make it so that by default all roles can see the Hidden Fields in the back-end. Note that this includes the
            ////"Authenticated" role, which is by default assigned to all created users. A consequence of this is that all users
            ////accessing the back-end will be able to see the values of the hidden fields. It is up to the administrator to 
            ////selectively remove this permission.
            //var allRoles = _roleService.GetRoles();
            //foreach (var role in allRoles) {
            //    _roleService.CreatePermissionForRole(role.Name, HiddenFieldsPermissions.MaySeeHiddenFields.Name);
            //}

            return 1;
        }
    }
}