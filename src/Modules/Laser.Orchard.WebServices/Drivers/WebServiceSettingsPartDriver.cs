using Laser.Orchard.WebServices.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.WebServices.Drivers {
    public class WebServiceSettingsPartDriver : ContentPartDriver<WebServiceSettingsPart> {
        private const string TemplateName = "Parts/WebServiceSettings";

        protected override string Prefix { get { return "WebServiceSettingsPartDriver"; } }

        protected override DriverResult Editor(WebServiceSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_WebServiceSettings_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: TemplateName,
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(WebServiceSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(part, Prefix, null, null)) {
                 }
           
            return Editor(part, shapeHelper);
        }
    }
}