using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.TenantBridges.Security {
    public class ExecuteRemoteRecipesPermission : IPermissionProvider {
        public static readonly Permission ExecuteRemoteRecipes = new Permission {
            Description = "Execute recipes remotely.",
            Name = "ExecuteRemoteRecipes"
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = GetPermissions().ToArray()
                }
            };
        }

        public IEnumerable<Permission> GetPermissions() {
            var permissions = new List<Permission>();
            permissions.Add(ExecuteRemoteRecipes);
            return permissions;
        }
    }
}