using Laser.Orchard.StartupConfig.Providers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Services {
    [OrchardFeature("Laser.Orchard.StartupConfig.JsonDataTablePart")]
    public class JsonDataTableService : IJsonDataTableService {
        private readonly IEnumerable<IJsonDataTableColumnProvider> _columnProviders;

        public JsonDataTableService(IEnumerable<IJsonDataTableColumnProvider> columnProviders) {
            _columnProviders = columnProviders;
        }

        public string ProcessColumnsDefinition(string columnsDefinition) {
            JArray cols = JArray.Parse(columnsDefinition);
            JArray newCols = new JArray();

            if (cols.HasValues) {
                foreach (var col in cols) {
                   foreach (var cp in _columnProviders) {
                        var newCol = cp.ProcessColumnDefinition(col);
                        newCols.Add(newCol);
                   }
                }
            }

            return JsonConvert.SerializeObject(newCols);
        }
            
    }
}