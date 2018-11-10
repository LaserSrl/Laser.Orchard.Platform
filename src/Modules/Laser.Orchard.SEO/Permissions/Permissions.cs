using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Laser.Orchard.SEO {
    public class Permissions : IPermissionProvider {

        public static readonly Permission ManageSEO = new Permission { Description = "Manage SEO", Name = "ManageSEO" };
        public static readonly Permission ConfigureRobotsTextFile = new Permission { Description = "Configure Robots.txt", Name = "ConfigureRobotsTextFile" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] { ManageSEO, ConfigureRobotsTextFile };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageSEO, ConfigureRobotsTextFile }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { ManageSEO }
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] { ManageSEO }
                }
            };
        }
    }
}