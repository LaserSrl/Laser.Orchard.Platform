using System.Text;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.StartupConfig.AppleEnvironment.Services;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.Security;

namespace Laser.Orchard.StartupConfig.AppleEnvironment.Controllers {
    [OrchardFeature("Laser.Orchard.ApplePay.DomainAssociation")]
    public class MerchantDomainAssociationController : Controller {
        private const string contentType = "text/plain";

        private readonly ICacheManager _cacheManager;
        private IMerchantDomainAssociationService _merchantDomainAssociationService;
        private readonly ISignals _signals;

        public MerchantDomainAssociationController(IMerchantDomainAssociationService merchantDomainAssociationService, ICacheManager cacheManager, ISignals signals) {
            _merchantDomainAssociationService = merchantDomainAssociationService;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        [AlwaysAccessible]
        public ActionResult Index() {       
            var content = _cacheManager.Get("MerchantDomainAssociation.Settings",
                              ctx => {
                                  ctx.Monitor(_signals.When("MerchantDomainAssociation.SettingsChanged"));
                                  var merchantDomainAssociationFile = _merchantDomainAssociationService.Get();
                                  return merchantDomainAssociationFile;
                              });
            
            if (!content.Enable) {
                return new HttpNotFoundResult();
            }

            return File(Encoding.UTF8.GetBytes(content.FileContent ?? ""), contentType, "apple-developer-merchantid-domain-association");
        }

    }
}