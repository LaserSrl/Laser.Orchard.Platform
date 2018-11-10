using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions.Models;

namespace Laser.Orchard.DataProtection.Security {
    public class DataProtectionPermissions : IPermissionProvider {
        public static readonly Permission ManageDataProtection = new Permission { Description = "Manage Data Protection", Name = "ManageDataProtection" };
        public Feature Feature { get; set; }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageDataProtection }
                }
            };
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] { ManageDataProtection };
        }
    }
}