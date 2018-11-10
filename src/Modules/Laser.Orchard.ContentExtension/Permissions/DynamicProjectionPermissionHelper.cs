//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using Orchard.Security.Permissions;

//namespace Laser.Orchard.ContentExtension.Navigation {
//    public class DynamicProjectionPermissionHelper {

//        public static readonly Dictionary<string, Permission> PermissionsList = new Dictionary<string, Permission>();

//        public IEnumerable<Permission> GetPermissions() {
//            var permissions = new List<Permission>();
//            var menuParts = _contentManager.Query<DynamicProjectionPart, DynamicProjectionPartRecord>().Where(x => x.OnAdminMenu).List();
//            var permissionOfAll = new Permission { Category = "Dynamic Projection Permission", Description = "Manage all Dynamic Projections", Name = "ManageDynamicProjections" };
//            permissions.Add(permissionOfAll);
//            foreach (var menuPart in menuParts) {
//                var newpermission = new Permission { ImpliedBy = new[] { permissionOfAll }, Category = "Dynamic Projection Permission", Description = "Specific Dynamic Projection Permission : " + menuPart.AdminMenuText, Name = "DynamicProjectionPermission" + menuPart.Id.ToString() };
//                if (PermissionsList.ContainsKey(newpermission.Name))
//                    PermissionsList[newpermission.Name] = newpermission;
//                else
//                    PermissionsList.Add(newpermission.Name, newpermission);
//                permissions.Add(newpermission);
//            }

//            return permissions;
//        }
//    }
//}