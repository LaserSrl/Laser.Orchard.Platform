using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Controllers {
    public class CustomPosAdminController : PosAdminBaseController {
        protected override string SettingsShape => "CustomPosSettings";

        public CustomPosAdminController(IOrchardServices orchardServices,
            ISignals signals) : base(orchardServices, signals) {
        }

        protected override ContentPart GetSettingsPart() {
            return _orchardServices.WorkContext.CurrentSite.As<CustomPosSiteSettingsPart>();
        }
    }
}