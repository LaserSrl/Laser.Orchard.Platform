using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Laser.Orchard.Queries {

    public class Permissions : IPermissionProvider {
     //   public static readonly Permission CustomQuery = new Permission { Description = "Manage CustomQuery", Name = "CustomQuery" };
        public static readonly Permission UserQuery = new Permission { Description = "Manage UserQuery", Name = "UserQuery"} ;

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
      //          CustomQuery,
                UserQuery
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                // Permissions = new[] {CustomQuery,UserQuery}
                   Permissions = new[] {UserQuery}
                },
                new PermissionStereotype {
                    Name = "Editor",
                },
                new PermissionStereotype {
                    Name = "Moderator",
                  },
                new PermissionStereotype {
                    Name = "Author",
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }
    }
}