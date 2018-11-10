using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Laser.Orchard.CulturePicker.Services;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard;


namespace Laser.Orchard.CulturePicker.Controllers {
    public class AdminController : Controller {
        private readonly ICulturePickerSettingsService _culturePickerSettingsService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IOrchardServices _orchardServices;

        public Localizer T { get; set; }
        // GET: /Admin/
        public AdminController(ICulturePickerSettingsService culturePickerSettingsService, 
            IAuthenticationService authenticationService,
            IMembershipService membershipService, IOrchardServices orcharcServices) {
            _culturePickerSettingsService = culturePickerSettingsService;
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _orchardServices = orcharcServices;
            T = NullLocalizer.Instance;
        }

        [HttpGet]
        public ActionResult Settings() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Yout have to be an Administrator to edit Culture Picker settings!")))
                return new HttpUnauthorizedResult();
            var model = _culturePickerSettingsService.ReadSettings();
            return View(model);
        }
        [HttpPost]
        public ActionResult Settings(Models.SettingsModel model) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Yout have to be an Administrator to edit Culture Picker settings!")))
                return new HttpUnauthorizedResult();
            if (!ModelState.IsValid) {
                _orchardServices.Notifier.Error(T("Settings update failed: {0}", T("check your input!")));
                return View(model);
            }
            try {
                _culturePickerSettingsService.WriteSettings(model);
                _orchardServices.Notifier.Information(T("Culture Picker settings updated."));
                // I read again my model in order to its ids
                model = _culturePickerSettingsService.ReadSettings();
            } catch (Exception exception) {
                _orchardServices.Notifier.Error(T("Settings update failed: {0}", exception.Message));
            }

            return RedirectToActionPermanent("Settings");
        }
    }
}
