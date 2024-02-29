using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;
using oml = Orchard.MediaLibrary;

namespace Laser.Orchard.StartupConfig.Security {
    [OrchardFeature("Laser.Orchard.StartupConfig.ExtendAdminControllerToFrontend")]
    public class FrontEndClientStoragePermissions : IPermissionProvider {
        public static readonly Permission FrontEndMediaUpload = new Permission {
            Description = "Front-end Media Upload",
            Name = "FrontEndMediaUpload",
            ImpliedBy = new[] { oml.Permissions.ManageMediaContent }
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                FrontEndMediaUpload
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {FrontEndMediaUpload}
                }
            };
        }
    }
}