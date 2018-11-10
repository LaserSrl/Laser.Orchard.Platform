using Laser.Orchard.UserProfiler.Models;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Http.Controllers;

namespace Laser.Orchard.UserProfiler.Drivers {

    public class TrackingPartDriver : ContentPartDriver<TrackingPart> {
        private readonly ShellSettings _shellsettings;
        private readonly IOrchardServices _orchardServices;

        public TrackingPartDriver(ShellSettings shellsettings, IOrchardServices orchardServices) {
            _shellsettings = shellsettings;
            _orchardServices = orchardServices;
        }

        protected override string Prefix {
            get { return "TrackingPart"; }
        }

        protected override DriverResult Display(TrackingPart part, string displayType, dynamic shapeHelper) {
            if (displayType.StartsWith("Summary"))
                return null;
            return ContentShape("Parts_Tracking_Display", () => shapeHelper.Parts_Tracking_Display(PageID: part.Id));
        }
    }
}