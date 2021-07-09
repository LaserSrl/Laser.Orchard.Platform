
using Laser.Orchard.StartupConfig.Services;
using Orchard.Data.Migration;
using System.Linq;
using Orchard.Roles.Services;
using Orchard;
using Orchard.Localization;

namespace Laser.Orchard.AdvancedSearch {
    public class Migrations : DataMigrationImpl {
        private readonly IUtilsServices _utilsServices;
        private readonly IRoleService _roleService;

        public Migrations(IUtilsServices utilsServices,
            IRoleService roleService,
            IOrchardServices orchardServices) {
            _utilsServices = utilsServices;
            _roleService = roleService;
        }

        public Localizer T { get; set; }

        public int Create() {
            return 1;
        }

        public int UpdateFrom1() {
            //update permissions on every custom role: to allow custom roles behaviour to still look as it did
            //before introducing the permissions in this module, we add all permissions to all custom roles.
            //Since AdvancedSearchPermissions.SeesAllContent implies all the others, it is enough to add that one.
            string[] defaultRoles = { "Administrator", "Editor", "Moderator", "Author", "Contributor", "Anonymous", "Authenticated" };
            var customRoles = _roleService.GetRoles().Where(r => !defaultRoles.Contains(r.Name));
            foreach (var role in customRoles) {
                //if the role is not a default role, add the permission
                _roleService.CreatePermissionForRole(role.Name, AdvancedSearchPermissions.SeesAllContent.Name);
            }

            return 2;
        }
    }
}