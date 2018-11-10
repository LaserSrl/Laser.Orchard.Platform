using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Security {
     [OrchardFeature("Laser.Orchard.StartupConfig.Maintenance")]
    public class MaintenancePermission:IPermissionProvider {


         public static readonly Permission Maintenance = new Permission { Description = "Maintenance", Name = "Maintenance", Category = "Maintenance Feature" };
        public MaintenancePermission() {
        }
        public virtual Feature Feature { get; set; }
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }
        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                 Maintenance
            };
        }
    }
}
