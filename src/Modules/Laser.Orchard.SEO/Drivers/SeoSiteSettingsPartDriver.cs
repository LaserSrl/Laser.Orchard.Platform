using Laser.Orchard.SEO.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Laser.Orchard.SEO.Drivers {
    public class SeoSiteSettingsPartDriver : ContentPartDriver<SeoSiteSettingsPart> {
        
        protected override string Prefix => "SeoSiteSettings";

        //GET
        protected override DriverResult Editor(SeoSiteSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_SeoSiteSettings_Edit",
                 () => shapeHelper.EditorTemplate(
                     TemplateName: "Parts/SeoSiteSettings",
                     Model: part,
                     Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(
            SeoSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}