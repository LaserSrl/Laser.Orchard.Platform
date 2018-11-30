using Laser.Orchard.GDPR.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Laser.Orchard.GDPR.Drivers {
    public class GDPRSiteSettingsDriver : ContentPartDriver<GDPRSiteSettingsPart> {
        private const string TemplateName = "Parts/GDPRSiteSettings";
        public Localizer T { get; set; }
        protected override string Prefix { get { return "GDPRSettings"; } }

        public GDPRSiteSettingsDriver() {
            T = NullLocalizer.Instance;
        }
        protected override DriverResult Editor(GDPRSiteSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }
        protected override DriverResult Editor(GDPRSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_GDPRSiteSettings_Edit", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            }).OnGroup("GDPR");
        }
    }
}