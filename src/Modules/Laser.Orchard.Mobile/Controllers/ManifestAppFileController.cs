using System.Text;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.Mobile.Services;
using Orchard.Caching;
using Orchard.Security;

namespace Laser.Orchard.Mobile.Controllers {
    public class ManifestAppFileController : Controller {
        private const string contentType = "text/plain";
        private const string contentTypeJson = "application/json"; 

        private readonly ICacheManager _cacheManager;
        private ManifestAppFileServices _manifestAppService;
        private readonly ISignals _signals;

        public ManifestAppFileController(ManifestAppFileServices manifestAppService, ICacheManager cacheManager, ISignals signals) {
            _manifestAppService = manifestAppService;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        [AlwaysAccessible]
        public ActionResult Index(AppType appType) {       
            var content = _cacheManager.Get("ManifestAppFile.Settings",
                              ctx => {
                                  ctx.Monitor(_signals.When("ManifestAppFile.SettingsChanged"));
                                  var manifestAppFile = _manifestAppService.Get();
                                  return manifestAppFile;
                              });

            if (appType == AppType.Apple) {
                if (!content.Enable) {
                    return new HttpNotFoundResult();
                }

                return File(Encoding.UTF8.GetBytes(content.FileContent ?? ""), contentType, "apple-app-site-association");
            }

            if (appType == AppType.Google) {
                if (!content.GoogleEnable) {
                    return new HttpNotFoundResult();
                }

                return File(Encoding.UTF8.GetBytes(content.GoogleFileContent ?? ""), contentTypeJson, "assetlinks.json");
            }
            return new HttpNotFoundResult();
        }

        [AlwaysAccessible]
        public ActionResult DeveloperDomain() {
            var content = _cacheManager.Get("ManifestAppFile.Settings",
                              ctx => {
                                  ctx.Monitor(_signals.When("ManifestAppFile.SettingsChanged"));
                                  var manifestAppFile = _manifestAppService.Get();
                                  return manifestAppFile;
                              });

            if (!content.EnableDeveloperDomain) {
                return new HttpNotFoundResult();
            }

            return File(Encoding.UTF8.GetBytes(content.DeveloperDomainText ?? ""), contentType, "apple-developer-domain-association.txt");
        }
    }
}