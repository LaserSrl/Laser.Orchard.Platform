using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Security {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopSynchronizationPermission : IPermissionProvider {
        public static readonly Permission FacebookShopSynchronization = new Permission {
            Description = "Synchronize products on Facebook shop.",
            Name = "FacebookShopSynchronization"
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
            permissions.Add(FacebookShopSynchronization);
            return permissions;
        }
    }
}