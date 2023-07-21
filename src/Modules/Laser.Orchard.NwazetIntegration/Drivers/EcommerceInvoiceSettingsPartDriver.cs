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
    public class EcommerceInvoiceSettingsPartDriver : ContentPartDriver<EcommerceInvoiceSettingsPart> {
        private const string TemplateName = "Parts/EcommerceInvoiceSettings";
        protected override string Prefix { get { return "EcommerceInvoiceSettings"; } }

        //GET
        protected override DriverResult Editor(EcommerceInvoiceSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_EcommerceInvoiceSettings_Edit",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: TemplateName,
                        Model: part,
                        Prefix: Prefix)).OnGroup("ECommerceSiteSettings");
        }

        //POST
        protected override DriverResult Editor(EcommerceInvoiceSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater != null) {
                updater.TryUpdateModel(part, Prefix, null, null);
            }
            return Editor(part, shapeHelper);
        }

        // managed import export of the invoice settings
        protected override void Exporting(EcommerceInvoiceSettingsPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("EnableInvoiceRequest", part.EnableInvoiceRequest.ToString());
        }

        protected override void Importing(EcommerceInvoiceSettingsPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }
            var enableInvoceRequest = false;
            bool.TryParse(context.Attribute(part.PartDefinition.Name, "EnableInvoiceRequest"), out enableInvoceRequest);
            part.EnableInvoiceRequest = enableInvoceRequest;
        }
    }
}
