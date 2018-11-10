using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Localization;
using Orchard.UI.Navigation;
namespace Orchard.Blogs {
    public class AdminMenu : INavigationProvider {
        public string MenuName {
            get { return "admin"; }
        }

        public AdminMenu() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Newsletters"), "1.6", menu => menu
                .Action("Index", "NewsletterAdmin", new { area = "Laser.Orchard.NewsLetters" })
                .AddImageSet("newsletter"));
        }

    }
}