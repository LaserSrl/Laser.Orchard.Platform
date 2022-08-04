using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Security {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointPermissions : IPermissionProvider {
        public static readonly Permission MayConfigurePickupPoints =
            new Permission {
                Description = "May configure pickup points.",
                Name = "MayConfigurePickupPoints"
            };

        public Feature Feature { get; set; }

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
            permissions.Add(MayConfigurePickupPoints);
            return permissions;
        }
    }
}