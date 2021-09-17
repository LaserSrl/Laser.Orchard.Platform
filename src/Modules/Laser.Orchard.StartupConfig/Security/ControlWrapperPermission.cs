using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Security {
    [OrchardFeature("Laser.Orchard.StartupConfig.ControlWrapper")]
    public class ControlWrapperPermission : IPermissionProvider {
        public static readonly Permission WidgetControlWrapper = new Permission { Description = "Widget Control Wrapper in front-end", Name = "WidgetControlWrapper" };
        public static readonly Permission ContentControlWrapper = new Permission { Description = "Content Control Wrapper in front-end", Name = "EditContentWrapper" };

        public Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                WidgetControlWrapper,
                ContentControlWrapper
            };
        }
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { WidgetControlWrapper, ContentControlWrapper }
                },
            };
        }
    }
}