using Orchard.Localization;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.MediaLibrary;

namespace Laser.Orchard.Vimeo.Providers {
    public class VimeoMenu : INavigationProvider {
        public Localizer T { get; set; }

        public VimeoMenu() {
            T = NullLocalizer.Instance;
        }

        public string MenuName { get { return "mediaproviders"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .Add(T("Upload to Vimeo"), "10",
                    menu => menu.Action("Index", "BackendUpload", new { area = "Laser.Orchard.Vimeo" })
                    .Permission(Permissions.ManageMediaContent));
        }
    }
}