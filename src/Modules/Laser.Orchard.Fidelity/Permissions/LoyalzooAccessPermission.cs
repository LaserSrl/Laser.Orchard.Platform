using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Laser.Orchard.Fidelity.Permissions
{
    public class LoyalzooAccessPermission : IPermissionProvider
    {
        public virtual Feature Feature { get; set; }

        public static readonly Permission AccessBackEnd = new Permission
        {
            Name = "AccessLoyalzooBackEnd",
            Description = "Access Loyalzoo Back End"
        };

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { AccessBackEnd }
                }
            };
        }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                 AccessBackEnd
            };
        }
    }
}