using Orchard.Localization;
using OrchardThemes = Orchard.Themes;
using Orchard.Themes.Services;
using Orchard.UI.Navigation;

namespace KrakeDefaultTheme.Settings {
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
                        .Add(T("Krake default theme options"), "5.1", item => item.Action("ThemeSettings", "Admin", new { area = Constants.RoutesAreaName }).LocalNav())
                    );
            }
        }
    }
}