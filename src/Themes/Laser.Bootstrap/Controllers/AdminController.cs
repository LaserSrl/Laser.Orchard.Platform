using System.Web.Mvc;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Notify;
using Laser.Bootstrap.ViewModels;
using Laser.Bootstrap.Services;

namespace Laser.Bootstrap.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IThemeSettingsService _settingsService;

        public AdminController(
            IOrchardServices services,
            IThemeSettingsService settingsService) {
            _settingsService = settingsService;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            var settings = _settingsService.GetSettings();
            var additionalThemes = _settingsService.GetThemes();
            var viewModel = new ThemeSettingsViewModel {
                Swatch = settings.Swatch,
                UseFixedNav = settings.UseFixedNav,
                UseNavSearch = settings.UseNavSearch,
                UseFluidLayout = settings.UseFluidLayout,
                UseInverseNav = settings.UseInverseNav,
                UseStickyFooter = settings.UseStickyFooter,
                TagLineText = settings.TagLineText,
                AdditionalThemes = additionalThemes,
            };

            return View("OptionsIndex", viewModel);
        }

        [HttpPost]
        public ActionResult Index(ThemeSettingsViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Bootstrap.Permissions.ManageThemeSettings, T("Couldn't update Bootstrap settings")))
                return new HttpUnauthorizedResult();

            var settings = _settingsService.GetSettings();
            viewModel.AdditionalThemes = _settingsService.GetThemes();
            settings.Swatch = viewModel.Swatch;
            settings.UseFixedNav = viewModel.UseFixedNav;
            settings.UseNavSearch = viewModel.UseNavSearch;
            settings.UseFluidLayout = viewModel.UseFluidLayout;
            settings.UseInverseNav = viewModel.UseInverseNav;
            settings.UseStickyFooter = viewModel.UseStickyFooter;
            settings.TagLineText = viewModel.TagLineText;

            Services.Notifier.Information(T("Your settings have been saved."));

            return View("OptionsIndex", viewModel);
        }
    }
}
