using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SecureData.Security {
    public class EncryptedStringFieldPermissions : IPermissionProvider {
        public static readonly Permission ManageAllEncryptedStringFields = new Permission {
            Description = "Manage all encrypted string fields",
            Name = "ManageAllEncryptedStringFields"
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = GetPermissions().ToArray()
                },
            };
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[]
            {
                ManageAllEncryptedStringFields
            };
        }
    }
}