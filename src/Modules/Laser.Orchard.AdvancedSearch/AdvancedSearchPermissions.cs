using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Laser.Orchard.AdvancedSearch {
    public class AdvancedSearchPermissions : IPermissionProvider {

        //With this permission, a user visualizes all contents by default. This overrides the next two permissions.
        public static readonly Permission SeesAllContent = new Permission {
            Description = "A user with this permission sees all content by default.",
            Name = "SeesAllContent"
        };
        //With this permission, a user initially can only visualize their own contents, but may choose to be able to see contents belonging
        //to other users.
        public static readonly Permission MayChooseToSeeOthersContent = new Permission {
            Description = "A user with this permission may choose to see other users' contents, but does not by default.",
            Name = "MayChooseToSeeOthersContent",
            ImpliedBy = new[] { SeesAllContent }
        };
        //With this permission, a user is able to visualize their own contents. They are unable to see anything they do not own,
        //unless another permission is active for them to do so.
        public static readonly Permission CanSeeOwnContents = new Permission {
            Description = "A user with this permission can see the content they own.",
            Name = "CanSeeOwnContents",
            ImpliedBy = new [] { MayChooseToSeeOthersContent }
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                SeesAllContent,
                MayChooseToSeeOthersContent,
                CanSeeOwnContents,
            };
        }

        //update stereotypes of default roles
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[]{
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { SeesAllContent }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { SeesAllContent }
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] { CanSeeOwnContents }
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] { CanSeeOwnContents }
                },
            };
        }
    }
}