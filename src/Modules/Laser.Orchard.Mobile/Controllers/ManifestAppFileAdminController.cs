using Laser.Orchard.Mobile.PermissionsManifestAppFile;
using Laser.Orchard.Mobile.Services;
using Laser.Orchard.Mobile.ViewModels;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.Mobile.Controllers {
    [Admin]
    [ValidateInput(false)]
    public class ManifestAppFileAdminController : Controller {
        private readonly IManifestAppFileServices _manifestAppService;

        public ManifestAppFileAdminController(IManifestAppFileServices manifestAppService, IOrchardServices orchardServices) {
            _manifestAppService = manifestAppService;
            Services = orchardServices;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!Authorized())
                return new HttpUnauthorizedResult();
            var manifest = _manifestAppService.Get();
            return View(new ManifestAppFileViewModel() {
                Text = manifest.FileContent,
                Enable = manifest.Enable,
                DeveloperDomainText = manifest.DeveloperDomainText,
                EnableDeveloperDomain = manifest.EnableDeveloperDomain
            });
        }

        [HttpPost]
        public ActionResult Index(ManifestAppFileViewModel viewModel) {
            if (!Authorized())
                return new HttpUnauthorizedResult(); 
            var saveResult = _manifestAppService.Save(viewModel);
            if (saveResult.Item1)
                Services.Notifier.Information(T("File settings successfully saved."));
            else {
                Services.Notifier.Error(T("File not saved."));
                saveResult.Item2.ToList().ForEach(error =>
                    Services.Notifier.Warning(T(error))
                );
            }
            return View(viewModel);
        }

        private bool Authorized() {
            return Services.Authorizer.Authorize(ManifestAppFilePermissions.ManifestAppFile, T("Cannot manage apple manifest file"));
        }
    }
}