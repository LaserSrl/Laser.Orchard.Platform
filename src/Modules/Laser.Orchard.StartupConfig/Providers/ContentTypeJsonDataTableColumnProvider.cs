using Laser.Orchard.StartupConfig.Models;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Providers {
    [OrchardFeature("Laser.Orchard.StartupConfig.JsonDataTablePart")]
    public class ContentTypeJsonDataTableColumnProvider : IJsonDataTableColumnProvider {
        private static string ColumnEditor = "ContentType";
        private static IContentManager _contentManager;

        public ContentTypeJsonDataTableColumnProvider(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public JToken ProcessColumnDefinition(JToken columnDefinition) {
            var columnEditor = columnDefinition.Value<string>("editor");

            if (CheckColumnEditor(columnEditor)) {
                var editorParams = columnDefinition.Value<JToken>("editorParams");

                if (editorParams != null) {
                    var ct = editorParams.Value<string>("ct");

                    if (!string.IsNullOrWhiteSpace(ct)) {
                        var v = editorParams.Value<string>("v");
                        var version = VersionOptions.Published;
                        if (!string.IsNullOrWhiteSpace(v)) {
                            switch (v.ToLowerInvariant()) {
                                case "l":
                                case "latest":
                                    version = VersionOptions.Latest;
                                    break;

                                case "p":
                                case "published":
                                    version = VersionOptions.Published;
                                    break;

                                default:
                                    break;
                            }
                        }
                        var q = _contentManager
                            .Query()
                            .ForType(ct)
                            .ForVersion(version);
                        var results = q.List();
                        if (results != null) {
                            var arr = new JArray();

                            foreach (var ci in results) {
                                var ciTitle = ci.ContentManager.GetItemMetadata(ci).DisplayText;
                                var el = new JObject();
                                el["value"] = ci.Id.ToString();
                                //el["value"] = ciTitle;
                                el["label"] = ciTitle;
                                arr.Add(el);
                            }

                            // Assign the list to "values" property
                            editorParams["values"] = arr;

                            // Refresh "editorParams" property of the "columnDefinition" JToken
                            columnDefinition["editorParams"] = editorParams;
                        }
                    }

                    // Add a marker to be replaced inside the shape to add a function to properly display the label of the selected item instead of its value.
                    columnDefinition["displayLabel"] = "label";
                    columnDefinition["sorter"] = "string";
                    columnDefinition["editor"] = "list";
                }
            }

            return columnDefinition;
        }

        public bool CheckColumnEditor(string editor) {
            return (!string.IsNullOrWhiteSpace(editor) && ColumnEditor.Equals(editor, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}