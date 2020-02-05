using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Maps.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.Maps.Drivers {
    [OrchardFeature("Laser.Orchard.Maps.OpenRoute")]
    public class OpenRouteSiteSettingsPartDriver : ContentPartDriver<OpenRouteSiteSettingsPart> {
        private const string TemplateName = "Parts/OpenRouteSiteSettings";

        public OpenRouteSiteSettingsPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "OpenRouteSettings"; } }

        protected override DriverResult Editor(OpenRouteSiteSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(OpenRouteSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_OpenRouteSiteSettings_Edit", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            })
            .OnGroup("OpenRouteService");
        }
    }
}