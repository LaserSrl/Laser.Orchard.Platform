using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Controllers {
    public class CustomPosAdminController : PosAdminBaseController {
        protected override string SettingsShape => "CustomPosAdminSettings";

        public CustomPosAdminController(IOrchardServices orchardServices,
            ISignals signals) : base(orchardServices, signals) {
        }

        protected override ContentPart GetSettingsPart() {
            var settings = _orchardServices.WorkContext.CurrentSite.As<CustomPosSiteSettingsPart>();
            if (settings.CustomPos == null) {
                settings.CustomPos = new System.Collections.Generic.List<CustomPosSettings>();
            }
            return settings;
        }
    }
}