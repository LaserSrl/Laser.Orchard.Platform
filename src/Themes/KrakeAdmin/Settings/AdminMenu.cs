using Orchard.Localization;
using OrchardThemes = Orchard.Themes;
using Orchard.Themes.Services;
using Orchard.UI.Navigation;

namespace KrakeAdmin.Settings {
    public class AdminMenu : INavigationProvider {
        private readonly ISiteThemeService _siteThemeService;

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public AdminMenu(ISiteThemeService siteThemeService) {
            _siteThemeService = siteThemeService;
        }

        public void GetNavigation(NavigationBuilder builder) {
            var themeName = _siteThemeService.GetCurrentThemeName();
            builder.Add(T("Themes"),
                menu => menu
                    .Add(T("Krake admin theme options"), "5.0", item => item.Action("ThemeSettings", "Admin", new { area = Constants.RoutesAreaName }).LocalNav())
                );
        }
    }
}