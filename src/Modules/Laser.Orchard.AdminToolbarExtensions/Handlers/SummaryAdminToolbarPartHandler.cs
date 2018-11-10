using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Laser.Orchard.AdminToolbarExtensions.Models;

namespace Laser.Orchard.AdminToolbarExtensions.Handlers {
    [OrchardFeature("Laser.Orchard.AdminToolbarExtensions")]
    public class SummaryAdminToolbarPartHandler : ContentHandler {

        public SummaryAdminToolbarPartHandler() {
            //OnUpdated<SummaryAdminToolbarPartSettings>((context, part) => {
            //    part.Labels = new List<SummaryAdminToolbarLabel>(part.Labels.Where(w=>!w.Delete);
            //});
        }
    }
}