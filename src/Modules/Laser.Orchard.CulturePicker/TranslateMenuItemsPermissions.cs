using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.CulturePicker {
    [OrchardFeature("Laser.Orchard.CulturePicker.TranslateMenuItems")]
    public class TranslateMenuItemsPermissions : IPermissionProvider {
        //Replay menu translation
        public static readonly Permission ReplayMenuTranslation = new Permission { Description = "Replay menu translation", Name = "ReplayMenuTranslation" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ReplayMenuTranslation,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ReplayMenuTranslation}
                },
            };
        }

    }
}