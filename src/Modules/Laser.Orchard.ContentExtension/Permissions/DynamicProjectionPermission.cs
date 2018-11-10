using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.ContentExtension.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Laser.Orchard.ContentExtension.Permissions {
    [OrchardFeature("Laser.Orchard.ContentExtension.DynamicProjection")]
    public class DynamicProjectionPermission : IPermissionProvider {
        public static readonly Dictionary<string, Permission> PermissionsList=new Dictionary<string, Permission>();

        private readonly IContentManager _contentManager;
        public DynamicProjectionPermission(IContentManager contentManager) {
            _contentManager = contentManager;

        }

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            var permissions = new List<Permission>();
            var menuParts = _contentManager.Query<DynamicProjectionPart, DynamicProjectionPartRecord>().Where(x => x.OnAdminMenu).List();
            var permissionOfAll = new Permission { Category = "Dynamic Projection Permission", Description = "Manage all Dynamic Projections", Name = "ManageDynamicProjections" };
            permissions.Add(permissionOfAll);
            foreach (var menuPart in menuParts) {
                var newpermission = new Permission { ImpliedBy = new[] { permissionOfAll }, Category = "Dynamic Projection Permission", Description = "Specific Dynamic Projection Permission : " + menuPart.AdminMenuText, Name = "DynamicProjectionPermission" + menuPart.Id.ToString() };
                if (PermissionsList.ContainsKey(newpermission.Name))
                    newpermission= PermissionsList[newpermission.Name];
                else
                    PermissionsList.Add(newpermission.Name, newpermission);
                permissions.Add(newpermission);
            }

            return permissions;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }
    }
}