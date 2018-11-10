using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Contrib.Profile {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ViewProfiles = new Permission { Description = "View profiles", Name = "ViewProfiles" };
        public static readonly Permission ViewOwnProfile = new Permission { Description = "View own profile", Name = "ViewOwnProfile", ImpliedBy = new [] { ViewProfiles }};
        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ViewProfiles,
                ViewOwnProfile
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Anonymous",
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] {ViewOwnProfile}
                },
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ViewProfiles}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {ViewOwnProfile}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] {ViewOwnProfile}
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {ViewOwnProfile}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {ViewOwnProfile}
                },
            };
        }

    }
}


