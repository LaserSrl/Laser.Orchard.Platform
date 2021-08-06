using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Drivers {
    public class AllowCrossOriginSettingsPartDriver : ContentPartDriver<AllowCrossOriginSettingsPart> {
        private const string TemplateName = "Parts/AllowCrossOriginSettings";

        protected override string Prefix { get { return "AllowCrossOriginSettingsPart"; } }

        protected override DriverResult Editor(AllowCrossOriginSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_AllowCrossOriginSettings_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: TemplateName,
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(AllowCrossOriginSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(part, Prefix, null, null)) {
            }

            return Editor(part, shapeHelper);
        }
    }
}