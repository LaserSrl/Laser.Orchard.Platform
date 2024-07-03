using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Providers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Services {
    [OrchardFeature("Laser.Orchard.StartupConfig.JsonDataTablePart")]
    public class JsonDataTableService : IJsonDataTableService {
        private readonly IEnumerable<IJsonDataTableColumnProvider> _columnProviders;
        private readonly IContentManager _contentManager;
        private IContentSerializationServices _contentSerializationServices;
        private readonly IWorkContextAccessor _workContextAccessor;

        private IContentSerializationServices contentSerializationServices {
            get {
                if (_contentSerializationServices == null) {
                    var workContext = _workContextAccessor.GetContext();
                    IContentSerializationServices services;
                    if (workContext.TryResolve(out services)) {
                        _contentSerializationServices = services;
                    }
                }

                return _contentSerializationServices;
            }
        }

        public JsonDataTableService(IEnumerable<IJsonDataTableColumnProvider> columnProviders,
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor) {

            _columnProviders = columnProviders;
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
        }

        public string ProcessColumnsDefinition(string columnsDefinition) {
            JArray cols = JArray.Parse(columnsDefinition);
            JArray newCols = new JArray();

            if (cols.HasValues) {
                foreach (var col in cols) {
                    JToken newCol = col;
                    foreach (var cp in _columnProviders) {
                        newCol = cp.ProcessColumnDefinition(newCol);
                    }
                    newCols.Add(newCol);
                }
            }

            var columns = JsonConvert.SerializeObject(newCols);

            // If column is set up to display the label, add a proper formatter function that checks inside the values in the list to display the label instead of the value for selected items.
            columns = columns.Replace("\"displayLabel\":\"label\"",
                "formatter:function(cell){var l = ''; cell.getColumn().getDefinition().editorParams.values.forEach(function(a){if (a.value.toString() == cell.getValue()) {l = a.label;} });return l;}");

            return columns;
        }

        public string ParseTableDataForExport(JsonDataTableField field) {
            var tableData = field.TableData;
            // First, check if each of the columns needs to be processed, based on column definition.
            var settings = field.PartFieldDefinition.Settings.GetModel<JsonDataTableFieldSettings>();
            JArray cols = JArray.Parse(settings.ColumnsDefinition);
            if (cols.HasValues) {
                foreach (var col in cols) {
                    var columnEditor = col.Value<string>("editor");
                    if (_columnProviders.Any(cp => cp.CheckColumnEditor(columnEditor))) {
                        // This column has got ids to be changed to identity
                        // Assume that every custom providers treats the data as content item ids.
                        var fieldName = col.Value<string>("field");
                        JArray rows = JArray.Parse(field.TableData);
                        foreach (var row in rows) {
                            var fieldValue = row.Value<string>(fieldName);
                            if (!string.IsNullOrWhiteSpace(fieldValue)) {
                                // fieldValue is the id of the selected content item
                                var intValue = 0;
                                if (int.TryParse(fieldValue, out intValue) && intValue > 0) {
                                    var ci = _contentManager.Get(intValue);
                                    var identity = ci.ContentManager.GetItemMetadata(ci).Identity;

                                    var identityString = HttpUtility.JavaScriptStringEncode(identity.ToString());
                                    tableData = tableData.Replace("\"" + fieldName + "\":\"" + fieldValue + "\"",
                                        "\"" + fieldName + "\":\"" + identityString + "\"");
                                }
                            }
                        }
                    }
                }
            }
            return tableData;
        }

        public string ParseTableDataForImport(JsonDataTableField field, string tableData, ImportContentContext context) {
            // First, check if each of the columns needs to be processed, based on column definition.
            var settings = field.PartFieldDefinition.Settings.GetModel<JsonDataTableFieldSettings>();
            JArray cols = JArray.Parse(settings.ColumnsDefinition);
            if (cols.HasValues) {
                foreach (var col in cols) {
                    var columnEditor = col.Value<string>("editor");
                    if (_columnProviders.Any(cp => cp.CheckColumnEditor(columnEditor))) {
                        // This column has got ids to be changed to identity
                        // Assume that every custom providers treats the data as content item ids.
                        var fieldName = col.Value<string>("field");
                        JArray rows = JArray.Parse(tableData);
                        foreach (var row in rows) {
                            var fieldValue = row.Value<string>(fieldName);
                            if (!string.IsNullOrWhiteSpace(fieldValue)) {
                                // fieldValue is the identity of the selected content item
                                // For this reason, we need to check if there is the proper content item in the target tenant (the one we are importing data to)
                                var intId = 0;

                                var ci = context.GetItemFromSession(fieldValue);
                                if (ci != null) {
                                    intId = ci.Id;
                                }

                                // Replace the identity with the integer id.
                                tableData = tableData.Replace("\"" + fieldName + "\":\"" + HttpUtility.JavaScriptStringEncode(fieldValue) + "\"",
                                        "\"" + fieldName + "\":\"" + intId.ToString() + "\"");
                            }
                        }
                    }
                }
            }

            return tableData;
        }

        public JArray SerializeData(JsonDataTableField field) {
            var elements = new JArray();
            // First, check if each of the columns needs to be processed, based on column definition.
            var settings = field.PartFieldDefinition.Settings.GetModel<JsonDataTableFieldSettings>();
            JArray cols = JArray.Parse(settings.ColumnsDefinition);
            if (cols.HasValues) {
                JArray rows = JArray.Parse(field.TableData);
                foreach (var row in rows) {
                    var element = new JObject();
                    foreach (var col in cols) {
                        var columnEditor = col.Value<string>("editor");
                        var fieldName = col.Value<string>("field");
                        if (_columnProviders.Any(cp => cp.CheckColumnEditor(columnEditor)) &&
                            contentSerializationServices != null) {
                            // Assume that every custom providers treats the data as content item ids.
                            // Content items need to be serialized in place of their ids.                        

                            var fieldValue = row.Value<string>(fieldName);
                            if (!string.IsNullOrWhiteSpace(fieldValue)) {
                                var intValue = 0;
                                if (int.TryParse(fieldValue, out intValue) && intValue > 0) {
                                    var ci = _contentManager.Get(intValue);
                                    var contentJson = contentSerializationServices.GetJson(ci);
                                    element[fieldName] = contentJson;
                                }
                            }
                        } else {
                            // Standard name-value column
                            var fieldValue = row.Value<string>(fieldName);
                            element[fieldName] = fieldValue;
                        }
                    }
                    elements.Add(element);
                }
            }

            return elements;
        }
    }
}