using Lasergroup.Extensions;
using Orchard.Localization;
using Orchard.Themes.Services;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lasergroup.Navigation {
    public class ThemeSettingsMenu : INavigationProvider {

        private readonly ISiteThemeService _siteThemeService;
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public ThemeSettingsMenu(
            ISiteThemeService siteThemeService) {

            _siteThemeService = siteThemeService;
        }

        public void GetNavigation(NavigationBuilder builder) {
            var themeName = _siteThemeService.GetSiteTheme();
            if (themeName.Name == Constants.ThemeName) {
                builder.Add(T("Themes"),
                    menu => menu
                        .Add(T("Frontend Theme settings"), "5.1", item => item.Action("Index", "Admin", new { area = Constants.RoutesAreaName }).LocalNav())
                    );
            }
        }

    }
}