using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;


namespace Laser.Orchard.PrivateMedia {
    public class PrivateMediaPermissions : IPermissionProvider {

        public static readonly Permission AccessAllPrivateMedia = new Permission {
            Description = "Access all private Media",
            Name = "AccessAllPrivateMedia",
            Category = "Private Media"
        };
        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                AccessAllPrivateMedia,
            };
        }

        //update stereotypes of default roles
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }
    }
}