using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Contrib.Widgets {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageTechnicalAttributesForContainerWidgets = new Permission { Description = "Managing technical Container Widgets", Name = "ManageTechnicalAttributesForContainerWidgets" };
        public static readonly Permission ManageContainerWidgets = new Permission { Description = "Managing Container Widgets", Name = "ManageContainerWidgets", ImpliedBy = new[] { ManageTechnicalAttributesForContainerWidgets , Orchard.Widgets.Permissions.ManageWidgets} };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageTechnicalAttributesForContainerWidgets,
                ManageContainerWidgets
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageTechnicalAttributesForContainerWidgets, ManageContainerWidgets }
                },
            };
        }

    }
}