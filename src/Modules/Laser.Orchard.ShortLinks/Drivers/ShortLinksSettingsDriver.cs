using Laser.Orchard.ShortLinks.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ShortLinks.Drivers {
    public class ShortLinksSettingsDriver : ContentPartDriver<ShortLinksSettingsPart> {

     
        //GET
        protected override DriverResult Editor(ShortLinksSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_ShortLinksSettings_Edit",
                 () => shapeHelper.EditorTemplate(
                     TemplateName: "Parts/ShortLinksSettings",
                     Model: part,
                     Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(
            ShortLinksSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

    }

}