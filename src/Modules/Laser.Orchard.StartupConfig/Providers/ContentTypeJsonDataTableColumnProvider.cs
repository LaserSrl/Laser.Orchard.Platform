using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
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

            if (!string.IsNullOrEmpty(columnEditor) && ColumnEditor.Equals(columnEditor, System.StringComparison.OrdinalIgnoreCase)) {
                var editorParams = columnDefinition.Value<JToken>("editorParams");

                if (editorParams != null) {
                    var ct = editorParams.Value<string>("ct");

                    if (!string.IsNullOrWhiteSpace(ct)) {
                        var v = editorParams.Value<string>("v");
                        var version = VersionOptions.Published;
                        if (!string.IsNullOrWhiteSpace(v)) {
                            switch (v.ToLowerInvariant()) {
                                case "l":
                                    version = VersionOptions.Latest;
                                    break;

                                default:
                                    break;
                            }
                        }
                        var q = _contentManager.Query().ForType(ct).ForVersion(version);
                        var results = q.List();
                        if (results != null) {
                            var arr = new JArray();

                            foreach (var ci in results) {
                                var ciTitle = ci.ContentManager.GetItemMetadata(ci).DisplayText;
                                var el = new JObject();
                                el["value"] = ci.Id.ToString();
                                el["label"] = ciTitle;
                                arr.Add(el);
                            }

                            // Assign the list to "values" property
                            editorParams["values"] = arr;

                            // Refresh "editorParams" property of the "columnDefinition" JToken
                            columnDefinition["editorParams"] = editorParams;
                        }
                    }

                    columnDefinition["editor"] = "list";
                }
            }

            return columnDefinition;
        }
    }
}