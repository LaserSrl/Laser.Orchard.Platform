using Laser.Orchard.FidelityGateway.Models;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.FidelityGateway.Drivers
{
    public class FidelityUserPartDriver:ContentPartDriver<FidelityUserPart>
    {
        protected override void Exporting(FidelityUserPart part, global::Orchard.ContentManagement.Handlers.ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("FidelityUsername", part.FidelityUsername);
            root.SetAttributeValue("FidelityPassword", part.FidelityPassword);
            root.SetAttributeValue("CustomerId", part.CustomerId);
        }
        protected override void Importing(FidelityUserPart part, global::Orchard.ContentManagement.Handlers.ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            var FidelityUsername = root.Attribute("FidelityUsername");
            if (FidelityUsername != null) {
                part.FidelityUsername = FidelityUsername.Value;
            }
            var FidelityPassword = root.Attribute("FidelityPassword");
            if (FidelityPassword != null) {
                part.FidelityPassword = FidelityPassword.Value;
            }
            var CustomerId = root.Attribute("CustomerId");
            if (CustomerId != null) {
                part.CustomerId = CustomerId.Value;
            }
        }
    }
}