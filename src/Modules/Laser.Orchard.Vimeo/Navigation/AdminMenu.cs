using Laser.Orchard.Vimeo.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Navigation {
    public class AdminMenu : INavigationProvider {

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"), menu => menu
                .Add(T("Vimeo"), "10.0", subMenu => subMenu
                    .Action("Index", "Admin", new { area = Constants.LocalArea })
                    .Permission(StandardPermissions.SiteOwner)
                    .Add(T("Settings"), "10.0", item => item
                        .Action("Index", "Admin", new { area = Constants.LocalArea })
                        .Permission(StandardPermissions.SiteOwner)
                        .LocalNav())
                ));
        }
    }
}