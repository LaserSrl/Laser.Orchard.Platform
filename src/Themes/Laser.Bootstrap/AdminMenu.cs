using Orchard.Localization;
using OrchardThemes = Orchard.Themes;
using Orchard.Themes.Services;
using Orchard.UI.Navigation;

namespace Laser.Bootstrap {
    public class AdminMenu : INavigationProvider {
        private readonly ISiteThemeService _siteThemeService;

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public AdminMenu(ISiteThemeService siteThemeService) {
            _siteThemeService = siteThemeService;
        }

        public void GetNavigation(NavigationBuilder builder) {
            var themeName = _siteThemeService.GetSiteTheme();
            if (themeName.Name == Constants.ThemeName) {
                builder.Add(T("Themes"),
                    menu => menu
                        .Add(T("Theme Options"), "1.0", item => item.Action("Index", "Admin", new { area = Constants.RoutesAreaName }).LocalNav())
                    );
            }
        }
    }
}