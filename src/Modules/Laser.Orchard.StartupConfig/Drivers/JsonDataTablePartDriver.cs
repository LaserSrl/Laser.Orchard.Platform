using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Drivers {
    [OrchardFeature("Laser.Orchard.StartupConfig.JsonDataTablePart")]
    public class JsonDataTablePartDriver : ContentPartDriver<JsonDataTablePart> {
        protected override DriverResult Display(JsonDataTablePart part, string displayType, dynamic shapeHelper) {
            if(displayType == "Detail") {
                var settings = part.Settings.GetModel<JsonDataTablePartSettings>();
                return ContentShape("Parts_JsonDataTable", () => shapeHelper.Parts_JsonDataTable(Table: part, Settings: settings));
            } else {
                return null;
            }
        }
        protected override DriverResult Editor(
        JsonDataTablePart part, dynamic shapeHelper) {
            return ContentShape("Parts_JsonDataTable_Edit",
            () => shapeHelper.EditorTemplate(
            TemplateName: "Parts/JsonDataTable",
            Model: part,
            Prefix: Prefix));
        }
        protected override DriverResult Editor(
        JsonDataTablePart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
        protected override void Importing(JsonDataTablePart part, ImportContentContext context) {
            context.ImportAttribute(part.PartDefinition.Name, "TableData", x => part.TableData = x);
        }
        protected override void Exporting(JsonDataTablePart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("TableData", part.TableData);
        } 
    }
}