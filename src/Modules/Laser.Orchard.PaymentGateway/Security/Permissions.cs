using Orchard.Security.Permissions;
using Orchard.Environment.Extensions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.Security {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ConfigurePayment = new Permission { Description = "Configure Payment POS", Name = "ConfigurePayment" };
        public Feature Feature { get; set; }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            Permission[] noPermission = new Permission[0];
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ConfigurePayment }
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
            return new[] { ConfigurePayment };
        }
    }
}