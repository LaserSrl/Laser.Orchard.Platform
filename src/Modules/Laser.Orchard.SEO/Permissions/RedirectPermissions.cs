using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Laser.Orchard.SEO {
    [OrchardFeature("Laser.Orchard.Redirects")]
    public class RedirectPermissions : IPermissionProvider {
        public virtual Feature Feature { get; set; }

        public static readonly Permission ConfigureRedirects =
            new Permission { Description = "Configure Redirects", Name = "ConfigureRedirects" };

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] { new PermissionStereotype { Name = "Administrator", Permissions = new[] { ConfigureRedirects } } };
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] { ConfigureRedirects };
        }
    }
}