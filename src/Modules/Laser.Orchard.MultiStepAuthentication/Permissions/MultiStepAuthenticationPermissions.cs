using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MultiStepAuthentication.Permissions {
    [OrchardFeature("Laser.Orchard.MultiStepAuthentication")]
    public class MultiStepAuthenticationPermissions : IPermissionProvider {

        public static readonly Permission ConfigureAuthentication = new Permission {
            Description = "Configure multi-step authentication processes",
            Name = "ConfigureAuthentication"
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] { ConfigureAuthentication };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ConfigureAuthentication }
                }
            };
        }
        
    }
}