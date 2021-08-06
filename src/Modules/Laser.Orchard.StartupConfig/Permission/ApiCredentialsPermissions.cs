using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.Security {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class ApiCredentialsPermissions : IPermissionProvider {
        public static readonly Permission ManageApiCredentials = new Permission {
            Description = "Manage Api Credentials",
            Name = "ManageApiCredentials"
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageApiCredentials
            };
        }
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageApiCredentials }
                },
            };
        }
    }
}