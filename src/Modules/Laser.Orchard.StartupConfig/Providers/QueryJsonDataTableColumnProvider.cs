using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Providers {
    [OrchardFeature("Laser.Orchard.StartupConfig.JsonDataTablePart")]
    public class QueryJsonDataTableColumnProvider : IJsonDataTableColumnProvider {
        private static string ColumnEditor = "Query";
        private readonly IContentManager _contentManager;
        private readonly IProjectionManager _projectionManager;

        public QueryJsonDataTableColumnProvider(IContentManager contentManager, 
            IProjectionManager projectionManager) {

            _contentManager = contentManager;
            _projectionManager = projectionManager;
        }

        public JToken ProcessColumnDefinition(JToken columnDefinition) {
            var columnEditor = columnDefinition.Value<string>("editor");

            if (!string.IsNullOrEmpty(columnEditor) && ColumnEditor.Equals(columnEditor, System.StringComparison.OrdinalIgnoreCase)) {
                var editorParams = columnDefinition.Value<JToken>("editorParams");

                if (editorParams != null) {
                    var queryName = editorParams.Value<string>("query");

                    if (!string.IsNullOrWhiteSpace(queryName)) {
                        var q = _contentManager
                            .Query()
                            .ForPart<QueryPart>()
                            .ForVersion(VersionOptions.Published)
                            .List()
                            .FirstOrDefault(qp => qp.Name.Equals(queryName, System.StringComparison.OrdinalIgnoreCase));
                        if (q != null) {
                            var results = _projectionManager.GetContentItems(q.Id);
                            if (results != null) {
                                var arr = new JArray();

                                foreach (var ci in results) {
                                    var ciTitle = ci.ContentManager.GetItemMetadata(ci).DisplayText;
                                    var el = new JObject();
                                    //el["value"] = ci.Id.ToString();
                                    el["value"] = ciTitle;
                                    el["label"] = ciTitle;
                                    arr.Add(el);
                                }

                                // Assign the list to "values" property
                                editorParams["values"] = arr;

                                // Refresh "editorParams" property of the "columnDefinition" JToken
                                columnDefinition["editorParams"] = editorParams;
                            }
                        }
                        columnDefinition["sorter"] = "string";
                        columnDefinition["editor"] = "list";
                    }
                }
            }

            return columnDefinition;
        }
    }
}