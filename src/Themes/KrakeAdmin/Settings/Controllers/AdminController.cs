using KrakeAdmin.Settings.Models;
using KrakeAdmin.Settings.ViewModels;
using KrakeAdmin.Settings.Services;
using Orchard;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;
using System.Web.Mvc;

namespace KrakeAdmin.Settings.Controllers {
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
                PlaceholderSmallLogoUrl = settings.PlaceholderSmallLogoUrl,
            };

            return View("~/Themes/KrakeAdmin/Views/admin/ThemeSettings.cshtml", viewModel);
        }

        [HttpPost]
        [ActionName("ThemeSettings")]
        public ActionResult ThemeSettingsPOST(ThemeSettingsViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't update theme settings")))
                return new HttpUnauthorizedResult();

            _settingsService.UpdateSettingsRecord(viewModel);
            Services.Notifier.Information(T("Your settings have been saved."));
            return RedirectToAction("ThemeSettings");
        }
    }

}
