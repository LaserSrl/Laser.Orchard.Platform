using Laser.Orchard.PaymentCartaSi.Models;
using Laser.Orchard.PaymentGateway.Controllers;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentCartaSi.Controllers {
    public class AdminController : PosAdminBaseController {

        public AdminController(IOrchardServices orchardServices,
            ISignals signals)
            : base(orchardServices, signals) {

        }

        protected override ContentPart GetSettingsPart() {
            return _orchardServices.WorkContext.CurrentSite.As<PaymentCartaSiSettingsPart>();
        }
    }
}