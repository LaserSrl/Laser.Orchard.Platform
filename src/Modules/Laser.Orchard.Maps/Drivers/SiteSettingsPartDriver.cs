using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Maps.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Laser.Orchard.Maps.Drivers {
    public class MapsSiteSettingsPartDriver : ContentPartDriver<MapsSiteSettingsPart> {
        private const string TemplateName = "Parts/MapsSiteSettings";

        public MapsSiteSettingsPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "MapsSettings"; } }

        protected override DriverResult Editor(MapsSiteSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(MapsSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_MapsSiteSettings_Edit", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            })
                .OnGroup("Maps");
        }
    }
}