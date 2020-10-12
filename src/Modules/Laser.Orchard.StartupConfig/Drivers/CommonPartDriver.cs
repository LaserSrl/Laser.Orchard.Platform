using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Drivers {
    public class CommonPartDriver : ContentPartDriver<CommonPart> {
        private readonly IOrchardServices _orchardServices;

        public CommonPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }
        protected override DriverResult Editor(CommonPart part, dynamic shapeHelper) {
            if (!AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext)) return null;
            return ContentShape("Parts_Common_SummaryInfoFor_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Common.SummaryInfoFor_Edit", Model: part, Prefix: Prefix));
        }
    }
}