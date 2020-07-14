using Laser.Orchard.PaymentGateway.Controllers;
using Laser.Orchard.PayPal.Models;
using Orchard;
using Orchard.ContentManagement;

namespace Laser.Orchard.PayPal.Controllers {
    public class AdminController : PosAdminBaseController {
        public AdminController(IOrchardServices orchardServices) : base(orchardServices) {
        }
        protected override ContentPart GetSettingsPart() {
            return _orchardServices.WorkContext.CurrentSite.As<PayPalSiteSettingsPart>();
        }
    }
}