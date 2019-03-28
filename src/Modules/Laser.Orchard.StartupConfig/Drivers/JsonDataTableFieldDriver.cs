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
    public class JsonDataTableFieldDriver : ContentFieldDriver<JsonDataTableField> {

        private static string GetPrefix(ContentField field, ContentPart part) {
            return (part.PartDefinition.Name + "." + field.Name)
                   .Replace(" ", "_");
        }

        private static string GetDifferentiator(ContentField field, ContentPart part) {
            return field.Name;
        }


        protected override DriverResult Display(ContentPart part, JsonDataTableField field, string displayType, dynamic shapeHelper) {
            if (displayType == "Detail") {
                var settings = field.PartFieldDefinition.Settings.GetModel<JsonDataTableFieldSettings>();
                return ContentShape("Fields_JsonDataTable", GetDifferentiator(field, part), () => shapeHelper.Fields_JsonDataTable(Table: field, Settings: settings));
            }
            else {
                return null;
            }
        }
        protected override DriverResult Editor(ContentPart part, JsonDataTableField field, dynamic shapeHelper) {
            return ContentShape("Fields_JsonDataTable_Edit", GetDifferentiator(field, part),
            () => shapeHelper.EditorTemplate(
            TemplateName: "Fields/JsonDataTable",
            Model: field,
            Prefix: GetPrefix(field, part)));
        }
        protected override DriverResult Editor(ContentPart part, JsonDataTableField field, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(field, GetPrefix(field, part), null, null);
            return Editor(part, field, shapeHelper);
        }
        protected override void Importing(ContentPart part, JsonDataTableField field, ImportContentContext context) {
            context.ImportAttribute(GetPrefix(field, part), "TableData", x => field.TableData = x);
        }
        protected override void Exporting(ContentPart part, JsonDataTableField field, ExportContentContext context) {
            context.Element(GetPrefix(field, part)).SetAttributeValue("TableData", field.TableData);
        }
    }
}