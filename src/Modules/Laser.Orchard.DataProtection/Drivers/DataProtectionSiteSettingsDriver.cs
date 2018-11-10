using Laser.Orchard.DataProtection.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.DataProtection.Drivers {
    public class DataProtectionSiteSettingsDriver : ContentPartDriver<DataProtectionSiteSettings> {
        private const string TemplateName = "Parts/DataProtectionSiteSettings";

        public DataProtectionSiteSettingsDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "DataProtectionSiteSettings"; } }

        protected override DriverResult Editor(DataProtectionSiteSettings part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(DataProtectionSiteSettings part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_DataProtectionSiteSettings_Edit", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            })
                .OnGroup("DataProtection");
        }
    }
}