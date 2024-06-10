using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Drivers {
    [OrchardFeature("Laser.Orchard.StartupConfig.JsonDataTablePart")]
    public class JsonDataTableFieldDriver : ContentFieldDriver<JsonDataTableField> {
        private readonly IJsonDataTableService _jsonDataTableService;

        public JsonDataTableFieldDriver(IJsonDataTableService jsonDataTableService) {
            _jsonDataTableService = jsonDataTableService;
        }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return (part.PartDefinition.Name + "." + field.Name)
                   .Replace(" ", "_");
        }

        private static string GetDifferentiator(ContentField field, ContentPart part) {
            return field.Name;
        }


        protected override DriverResult Display(ContentPart part, JsonDataTableField field, string displayType, dynamic shapeHelper) {
            if (displayType == "Detail") {
                return ContentShape("Fields_JsonDataTable",
                    GetDifferentiator(field, part),
                    () => {
                        var settings = field.PartFieldDefinition.Settings.GetModel<JsonDataTableFieldSettings>();
                        return shapeHelper.Fields_JsonDataTable(Table: field, Settings: settings);
                    });
            } else {
                return null;
            }
        }
        protected override DriverResult Editor(ContentPart part, JsonDataTableField field, dynamic shapeHelper) {
            return ContentShape("Fields_JsonDataTable_Edit", GetDifferentiator(field, part),
                () => {
                    var jdtfvm = new JsonDataTableFieldViewModel(field, "");
                   
                    var settings = field.PartFieldDefinition.Settings.GetModel<JsonDataTableFieldSettings>();
                    jdtfvm.ColumnsDefinition = _jsonDataTableService.ProcessColumnsDefinition(settings.ColumnsDefinition);

                    return shapeHelper.EditorTemplate(
                        TemplateName: "Fields/JsonDataTable",
                        Model: jdtfvm,
                        Prefix: GetPrefix(field, part));
                });
        }
        protected override DriverResult Editor(ContentPart part, JsonDataTableField field, IUpdateModel updater, dynamic shapeHelper) {
            // Specifying columnsDefinition parameter is not needed in this case, so pass an empty string as a parameter
            var jdtfvm  = new JsonDataTableFieldViewModel(field, "");

            updater.TryUpdateModel(jdtfvm, GetPrefix(field, part), null, null);

            return Editor(part, field, shapeHelper);
        }
        protected override void Importing(ContentPart part, JsonDataTableField field, ImportContentContext context) {
            context.ImportAttribute(GetPrefix(field, part), 
                "TableData", 
                x => field.TableData = _jsonDataTableService.ParseTableDataForImport(field, x, context));
        }
        protected override void Exporting(ContentPart part, JsonDataTableField field, ExportContentContext context) {
            var tableData = _jsonDataTableService.ParseTableDataForExport(field);            
            context.Element(GetPrefix(field, part)).SetAttributeValue("TableData", tableData);
        }
    }
}