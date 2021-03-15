using Laser.Orchard.Braintree.Models;
using Laser.Orchard.PaymentGateway.Controllers;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;

namespace Laser.Orchard.Braintree.Controllers {
    public class AdminController : PosAdminBaseController {
        public AdminController(IOrchardServices orchardServices,
            ISignals signals) : base(orchardServices, signals) {
        }
        protected override string CacheKey => BraintreeSiteSettingsPart.CacheKey;
        protected override ContentPart GetSettingsPart() {
            return _orchardServices.WorkContext.CurrentSite.As<BraintreeSiteSettingsPart>();
        }
    }
}