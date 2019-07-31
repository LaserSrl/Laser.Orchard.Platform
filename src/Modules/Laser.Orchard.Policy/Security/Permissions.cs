using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Laser.Orchard.Policy.Security {
    public class Permissions : IPermissionProvider {
        public static readonly Permission AddPolicyToRegistration = new Permission { Description = "Add policy to registration", Name = "AddPolicyToRegistration" };
        public Feature Feature { get; set; }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            Permission[] noPermission = new Permission[0];
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { AddPolicyToRegistration }
                },
                new PermissionStereotype {
                    Name = "Anonymous",
                    Permissions = noPermission
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = noPermission
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = noPermission
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = noPermission
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = noPermission
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = noPermission
                },
            };
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] { AddPolicyToRegistration };
        }
    }
}