//using Laser.Orchard.SEO.Services;
//using Laser.Orchard.SEO.ViewModels;
using Laser.Orchard.UsersExtensions.Services;
using Laser.Orchard.UsersExtensions.ViewModels;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.UsersExtensions.Controllers {
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
            //if (!Authorized())
            //    return new HttpUnauthorizedResult(); //TODO permission
            return View(new ManifestAppFileViewModel() { Text = _manifestAppService.Get().FileContent });
        }

        [HttpPost]
        public ActionResult Index(ManifestAppFileViewModel viewModel) {
            //if (!Authorized())
            //    return new HttpUnauthorizedResult(); //TODO permission
            var saveResult = _manifestAppService.Save(viewModel.Text);
            if (saveResult.Item1)
                Services.Notifier.Information(T("File settings successfully saved"));
            else {
                //Services.Notifier.Information(T("File saved with warnings"));
                //saveResult.Item2.ToList().ForEach(error =>
                //    Services.Notifier.Warning(T(error))
                //);
            }
            return View(viewModel);
        }

        //private bool Authorized() {
        //    return Services.Authorizer.Authorize(Permissions.ConfigureRobotsTextFile, T("Cannot manage robots.txt file"));
        //}
    }
}