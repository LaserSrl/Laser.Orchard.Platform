using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Laser.Orchard.Facebook {

    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageFacebookAccount = new Permission { Description = "Manage Social account", Name = "ManageFacebookAccount" };
        public static readonly Permission AdminFacebookAccount = new Permission { Description = "Manage all Social account", Name = "AdminFacebookAccount" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
               ManageFacebookAccount,
               AdminFacebookAccount
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                 Permissions = new[] {ManageFacebookAccount,AdminFacebookAccount}
                },
                new PermissionStereotype {
                    Name = "Editor",
                       Permissions = new[] {ManageFacebookAccount}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                  },
                new PermissionStereotype {
                    Name = "Author",
                       Permissions = new[] {ManageFacebookAccount}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }
    }
}