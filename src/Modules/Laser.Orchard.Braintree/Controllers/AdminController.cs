using Laser.Orchard.Braintree.Models;
using Laser.Orchard.PaymentGateway.Controllers;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Braintree.Controllers {
    public class AdminController : PosAdminBaseController {
        public AdminController(IOrchardServices orchardServices) : base(orchardServices) {
        }
        protected override ContentPart GetSettingsPart() {
            return _orchardServices.WorkContext.CurrentSite.As<BraintreeSiteSettingsPart>();
        }
    }
}