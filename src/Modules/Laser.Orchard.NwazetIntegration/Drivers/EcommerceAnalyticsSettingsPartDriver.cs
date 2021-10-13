using Laser.Orchard.Cookies;
using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
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

        // managed import export of the cookie level property because it is an enum and is not dynamically imported or exported
        protected override void Exporting(EcommerceAnalyticsSettingsPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("EcommerceCookieLevel", part.EcommerceCookieLevel.ToString());
        }

        protected override void Importing(EcommerceAnalyticsSettingsPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            part.EcommerceCookieLevel = EnumExtension<CookieType>.ParseEnum(context.Attribute(part.PartDefinition.Name, "EcommerceCookieLevel"));
        }
    }
}
