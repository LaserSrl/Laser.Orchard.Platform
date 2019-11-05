
using Framework = Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Laser.Orchard.Mobile.PermissionsManifestAppFile {
    public class ManifestAppFilePermissions : IPermissionProvider {

        public virtual Framework.Feature Feature { get; set; }

        public static readonly Permission ManifestAppFile =
            new Permission { Description = "Manifest App File", Name = "Manifest App File for Apple" };

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] { new PermissionStereotype { Name = "Administrator", Permissions = new[] { ManifestAppFile } } };
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] { ManifestAppFile };
        }
    }
}