using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields {
    public class HiddenFieldsPermissions : IPermissionProvider {
        
        //With this permission, we give admins the ability to modify the hidden fields' values in the back-end
        public static readonly Permission MayEditHiddenFields = new Permission {
            Description = "A user with this permission is able to edit the values in hidden fields. Be careful as that may have unwanted side effects.",
            Name = "MayEditHiddenFields"
        };
        //With this permission, we give users the ability to see the values in the hidden fields in the back-end
        public static readonly Permission MaySeeHiddenFields = new Permission {
            Description = "A user with this permission is able to see the values of the hidden fields.",
            Name = "MaySeeHiddenFields",
            ImpliedBy = new[] { MayEditHiddenFields }
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                MayEditHiddenFields,
                MaySeeHiddenFields,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype{
                    Name = "Administrator",
                    Permissions =  new[] { MayEditHiddenFields }
                },
            };
        }
    }
}