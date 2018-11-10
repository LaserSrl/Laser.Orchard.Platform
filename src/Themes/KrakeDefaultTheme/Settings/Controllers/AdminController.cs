using KrakeDefaultTheme.Settings.Models;
using KrakeDefaultTheme.Settings.ViewModels;
using KrakeDefaultTheme.Settings.Services;
using Orchard;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;
using System.Web.Mvc;

namespace KrakeDefaultTheme.Settings.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IThemeSettingsService _settingsService;
        public IOrchardServices Services { get; private set; }

        public AdminController(
            IThemeSettingsService settingsService,
            IOrchardServices services) {
            Services = services;
            _settingsService = settingsService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult ThemeSettings() {
            var settings = _settingsService.GetSettings();
            var viewModel = new ThemeSettingsViewModel {
                BaseLineText = settings.BaseLineText,
                HeaderLogoUrl = settings.HeaderLogoUrl,
                PlaceholderLogoUrl = settings.PlaceholderLogoUrl,

            };

            return View("~/Themes/KrakeDefaultTheme/Views/admin/ThemeSettings.cshtml", viewModel);
        }

        [HttpPost]
        [ActionName("ThemeSettings")]
        public ActionResult ThemeSettingsPOST(ThemeSettingsViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't update theme settings")))
                return new HttpUnauthorizedResult();

            var settings = _settingsService.GetSettings();
            settings.BaseLineText = viewModel.BaseLineText;
            settings.HeaderLogoUrl = viewModel.HeaderLogoUrl;
            settings.PlaceholderLogoUrl = viewModel.PlaceholderLogoUrl;

            Services.Notifier.Information(T("Your settings have been saved."));

            return RedirectToAction("ThemeSettings");
        }
    }

}
