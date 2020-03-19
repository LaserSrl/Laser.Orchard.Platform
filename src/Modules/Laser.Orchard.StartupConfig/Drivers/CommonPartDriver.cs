using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Drivers {
    public class CommonPartDriver : ContentPartDriver<CommonPart> {

        protected override DriverResult Editor(CommonPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Common_SummaryInfoFor_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Common.SummaryInfoFor_Edit", Model: part, Prefix: Prefix));
        }
    }
}