using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TaskScheduler.Navigation {
    public class AdminMenu : INavigationProvider {

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Task Scheduler"), "10", menu =>
                menu.Action("Index", "Admin", new { area = Constants.LocalArea })
                .Permission(StandardPermissions.SiteOwner)
                );
        }

    }
}