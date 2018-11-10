using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.GDPR.ViewModels {
    public class GDPRAdminViewModel {

        public dynamic ContentItems { get; set; }
        public dynamic Pager { get; set; }

        public string SearchExpression { get; set; }
    }
}