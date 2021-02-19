using Laser.Orchard.WebServices.Helpers;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System.Web.Mvc;

namespace Laser.Orchard.WebServices.Controllers {
    [OrchardFeature("Laser.Orchard.CustomRestApi")]
    [ValidateInput(false), Admin]
    public class CustomRestApiSettingsAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly ISiteService _siteService;
        
        // We are handling the settings for this feature through its own controller to easily expand
        // them with additional parts and drivers.
        public CustomRestApiSettingsAdminController(
            IOrchardServices orchardServices,
            ISiteService siteService) {

            _orchardServices = orchardServices;
            _siteService = siteService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, null, T("Not authorized to manage custom REST API settings")))
                return new HttpUnauthorizedResult();

            var site = _siteService.GetSiteSettings();
            dynamic model = _orchardServices.ContentManager.BuildEditor(site, CustomRestApiHelper.SettingsGroupId);

            if (model == null)
                return HttpNotFound();

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, null, T("Not authorized to manage custom REST API settings")))
                return new HttpUnauthorizedResult();

            var site = _siteService.GetSiteSettings();
            var model = _orchardServices.ContentManager.UpdateEditor(site, this, CustomRestApiHelper.SettingsGroupId);

            if (model == null) {
                _orchardServices.TransactionManager.Cancel();
                return HttpNotFound();
            }

            if (!ModelState.IsValid) {
                _orchardServices.TransactionManager.Cancel();

                return View(model);
            }
            _orchardServices.Notifier.Information(T("REST API settings updated"));

            return RedirectToAction("Index");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}