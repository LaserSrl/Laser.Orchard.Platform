using System.Text;
using System.Web.Mvc;
using Laser.Orchard.UsersExtensions.Services;
using Orchard.Caching;


namespace Laser.Orchard.UsersExtensions.Controllers {
    public class ManifestAppFileController : Controller {
        private const string ContentType = "text/plain";

        //private readonly IRobotsService _robotsService;
        private readonly ICacheManager _cacheManager;
        private ManifestAppFileServices _manifestAppService;
        private readonly ISignals _signals;

        public ManifestAppFileController(ManifestAppFileServices manifestAppService, ICacheManager cacheManager, ISignals signals) {
            _manifestAppService = manifestAppService;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public ContentResult Index() {
            var content = _cacheManager.Get("ManifestAppFile.Settings",
                              ctx => {
                                  ctx.Monitor(_signals.When("ManifestAppFile.SettingsChanged"));
                                  var manifestAppFile = _manifestAppService.Get();
                                  return manifestAppFile.FileContent;
                              });
            if (string.IsNullOrWhiteSpace(content)) {
                content = _manifestAppService.Get().FileContent;
            }
            return new ContentResult() {
                ContentType = ContentType,
                ContentEncoding = Encoding.UTF8,
                Content = content
            };
        }
    }
}