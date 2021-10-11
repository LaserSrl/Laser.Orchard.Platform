using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class EcommerceAnalyticsSettingsPartDriver : ContentPartDriver<EcommerceAnalyticsSettingsPart> {
        private const string TemplateName = "Parts/EcommerceAnalyticsSettings";
        protected override string Prefix { get { return "EcommerceAnalyticsSettings"; } }

        //GET
        protected override DriverResult Editor(EcommerceAnalyticsSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_EcommerceAnalyticsSettings_Edit",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: TemplateName,
                        Model: part,
                        Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(EcommerceAnalyticsSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater != null) {
                updater.TryUpdateModel(part, Prefix, null, null);
            }
            return Editor(part, shapeHelper);
        }
    }
}
