using Contrib.Profile.Services;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;
using System.Web.Mvc;

namespace Contrib.Profile.Controllers {
    [ValidateInput(false), Themed]
    public class HomeController : Controller, IUpdateModel {

        private readonly IMembershipService _membershipService;
        private readonly IContentManager _contentManager;
        private readonly IFrontEndProfileService _frontEndProfileService;

        public HomeController(IOrchardServices services,
            IMembershipService membershipService,
            IContentManager contentManager,
            IFrontEndProfileService frontEndProfileService) {

            _membershipService = membershipService;
            _contentManager = contentManager;
            _frontEndProfileService = frontEndProfileService;

            Services = services;
        }

        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index(string username) {
            IUser user = _membershipService.GetUser(username);

            if (user == null ||
                _frontEndProfileService.UserHasNoProfilePart(user) ||
                !Services.Authorizer.Authorize(Permissions.ViewProfiles, user, null)) {
                return HttpNotFound();
            }
            
            dynamic shape = ((IFrontEndEditService)_frontEndProfileService).BuildFrontEndShape(
                _contentManager.BuildDisplay(user, "", ""), //since the result of BuildDisplay is dynamic I have to do the ugly thing below
                _frontEndProfileService.MayAllowPartDisplay,
                _frontEndProfileService.MayAllowFieldDisplay);

            return View((object)shape);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Edit() {
            IUser user = Services.WorkContext.CurrentUser;

            if (user == null ||
                _frontEndProfileService.UserHasNoProfilePart(user)) {
                return HttpNotFound();
            }
            
            dynamic shape = ((IFrontEndEditService)_frontEndProfileService).BuildFrontEndShape(
                _contentManager.BuildEditor(user),
                _frontEndProfileService.MayAllowPartEdit,
                _frontEndProfileService.MayAllowFieldEdit);

            return View((object)shape);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost() {
            IUser user = Services.WorkContext.CurrentUser;

            if (user == null ||
                _frontEndProfileService.UserHasNoProfilePart(user)) {
                return HttpNotFound();
            }

            dynamic shape = ((IFrontEndEditService)_frontEndProfileService).BuildFrontEndShape(
                _contentManager.UpdateEditor(user, this),
                _frontEndProfileService.MayAllowPartEdit,
                _frontEndProfileService.MayAllowFieldEdit);
            
            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View("Edit", (object)shape);
            }

            Services.Notifier.Information(T("Your profile has been saved."));

            return RedirectToAction("Edit");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}