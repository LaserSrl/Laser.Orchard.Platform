using System.Collections.Generic;

using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Laser.Orchard.Sharing
{
    
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission EditSettings = new Permission { Description = "Edit AddThis settings", Name = "EditAddThisSettings" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                EditSettings
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {EditSettings}
                }
            };
        }

    }
}
