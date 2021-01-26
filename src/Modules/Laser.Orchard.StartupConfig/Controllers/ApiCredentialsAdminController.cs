using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Security;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.Controllers {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    [Admin]
    public class ApiCredentialsAdminController : Controller {

        private readonly IApiCredentialsManagementService _apiCredentialsMembershipService;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;
        private readonly IContentManager _contentManager;

        public ApiCredentialsAdminController(
            IApiCredentialsManagementService apiCredentialsMembershipService,
            IAuthorizer authorizer,
            INotifier notifier,
            IContentManager contentManager) {

            _apiCredentialsMembershipService = apiCredentialsMembershipService;
            _authorizer = authorizer;
            _notifier = notifier;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        [HttpPost]
        public ActionResult Generate(int userId, string returnUrl) {

            var userCredentials = _contentManager.Get<ApiCredentialsPart>(userId);
            if (userCredentials == null)
                return HttpNotFound();
            
            if (!_authorizer.Authorize(ApiCredentialsPermissions.ManageApiCredentials,
                userCredentials, T("Not authorized to manage API credentials."))) {
                return new HttpUnauthorizedResult();
            }
            // given the user ID
            // generate new credentials for the user
            _apiCredentialsMembershipService.GenerateNewCredentials(userCredentials);
            _notifier.Information(T("New credentials have been generated."));
            return this.RedirectLocal(returnUrl, 
                () => RedirectToAction("Edit", "Admin", new { Area = "Orchard.Users", Id = userId }));
        }
    }
}