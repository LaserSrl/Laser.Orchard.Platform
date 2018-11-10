using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Laser.Orchard.Twitter {

    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageTwitterAccount = new Permission { Description = "Manage Social twitter account", Name = "ManageTwitterAccount" };
        public static readonly Permission AdminTwitterAccount = new Permission { Description = "Manage all Social twitter account", Name = "AdminTwitterAccount" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
               ManageTwitterAccount,
               AdminTwitterAccount
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                 Permissions = new[] {ManageTwitterAccount,AdminTwitterAccount}
                },
                new PermissionStereotype {
                    Name = "Editor",
                       Permissions = new[] {ManageTwitterAccount}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                  },
                new PermissionStereotype {
                    Name = "Author",
                       Permissions = new[] {ManageTwitterAccount}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }
    }
}