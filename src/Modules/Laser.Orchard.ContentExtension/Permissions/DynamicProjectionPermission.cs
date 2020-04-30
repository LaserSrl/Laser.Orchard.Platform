using Laser.Orchard.ContentExtension.Models;
using Laser.Orchard.ContentExtension.Services;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.ContentExtension.Permissions {
    [OrchardFeature("Laser.Orchard.ContentExtension.DynamicProjection")]
    public class DynamicProjectionPermission : IPermissionProvider {
        private static readonly Permission ManageAll = new Permission {
            Category = "Dynamic Projection Permission",
            Description = "Manage all Dynamic Projections",
            Name = "ManageDynamicProjections"
        };
        
        private readonly IDynamicProjectionService _dynamicProjectionService;

        public DynamicProjectionPermission(
            IDynamicProjectionService dynamicProjectionService) {
            
            _dynamicProjectionService = dynamicProjectionService;
        }

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            var permissions = new List<Permission>();
            permissions.Add(ManageAll);
            // dynamic permissions
            var menuParts = _dynamicProjectionService.GetPartsForMenu();
            foreach (var menuPart in menuParts) {
                var newpermission = CreateDynamicPermission(menuPart);
                permissions.Add(newpermission);
            }

            return permissions;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

        public static Permission CreateDynamicPermission(DynamicProjectionPart part) {
            return new Permission {
                ImpliedBy = new[] { ManageAll },
                Category = "Dynamic Projection Permission",
                Description = "Specific Dynamic Projection Permission : " + part.AdminMenuText,
                Name = "DynamicProjectionPermission" + part.Id.ToString()
            };
        }
    }
}