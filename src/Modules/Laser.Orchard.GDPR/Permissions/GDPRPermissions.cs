using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.GDPR.Permissions {
    public class GDPRPermissions : IPermissionProvider {

        public static readonly Permission ManageAnonymization = new Permission {
            Description = "Allow invoking the anonymization of a ContentItem",
            Name = "ManageAnonymization"
        };

        public static readonly Permission ManageErasure = new Permission {
            Description = "Allow invoking the erasure of personal identifiable information from a ContentItem",
            Name = "ManageErasure"
        };

        public static readonly Permission ManageItemProtection = new Permission {
            Description = "Allow setting a ContentItem up as protected, so that anonymization and erasure will not affect it",
            Name = "ManageItemProtection"
        };

        public Feature Feature { get; set; }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageAnonymization,
                ManageErasure,
                ManageItemProtection
            };
        }
    }
}