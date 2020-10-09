using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;


namespace Laser.Orchard.StartupConfig.Security {
    [OrchardFeature("Laser.Orchard.StartupConfig.PerItemCache")]
    public class PerItemCachePermission : IPermissionProvider {

        public virtual Feature Feature { get; set; }

        public static readonly Permission AccessPerItemCacheKey = new Permission {
            Name = "AccessPerItemCacheKey",
            Description = "Enable to see the field for the cache key"
        };


        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { AccessPerItemCacheKey }
                }
            };
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                 AccessPerItemCacheKey
            };
        }
    }
}