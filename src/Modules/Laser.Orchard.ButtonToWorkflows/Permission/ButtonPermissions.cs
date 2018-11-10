using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.ButtonToWorkflows.Security {
    public class ButtonPermissions : IPermissionProvider {
        public static readonly Permission ButtonToWorkFlow1 = new Permission { Description = "ButtonToWorkFlow1", Name = "ButtonToWorkFlow1", Category = "Button to Workflow Feature" };
        public static readonly Permission ButtonToWorkFlow2 = new Permission { Description = "ButtonToWorkFlow2", Name = "ButtonToWorkFlow2", Category = "Button to Workflow Feature" };
        public static readonly Permission ButtonToWorkFlow3 = new Permission { Description = "ButtonToWorkFlow3", Name = "ButtonToWorkFlow3", Category = "Button to Workflow Feature" };
          public ButtonPermissions() {
        }
        public virtual Feature Feature { get; set; }
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }
        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                 ButtonToWorkFlow1,
                 ButtonToWorkFlow2,
                 ButtonToWorkFlow3
            };
        }
    }
}
