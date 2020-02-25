using Nwazet.Commerce.Permissions;
using Orchard.Localization;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Navigation {
    public class AddressSettingsAdminMenu : INavigationProvider {
        public string MenuName {
            get { return "admin"; }
        }

        public AddressSettingsAdminMenu() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"), menu => menu
                .Add(T("E-Commerce"), "2.1", subMenu => {
                    // add local navigation for self and submenus
                    subMenu.Permission(CommercePermissions.ManageCommerce);
                    subMenu.Add(T("Base"), "1.0", item => item
                        .Action("Index", "ECommerceSettingsAdmin", new { area = "Nwazet.Commerce" })
                        .LocalNav());
                    subMenu.Add(T("Address Configuration"), "2.0", item => item
                        .Action("Index", "AddressConfigurationAdmin", new { area = "Laser.Orchard.NwazetIntegration" })
                        .Permission(CommercePermissions.ManageCommerce)
                        .LocalNav());
                }));

        }
    }
}