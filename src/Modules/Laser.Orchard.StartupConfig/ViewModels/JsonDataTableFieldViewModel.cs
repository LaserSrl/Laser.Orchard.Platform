using Laser.Orchard.StartupConfig.Models;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.ViewModels {
    [OrchardFeature("Laser.Orchard.StartupConfig.JsonDataTablePart")]
    public class JsonDataTableFieldViewModel {
        public JsonDataTableField Field { get; set; }
        public string ColumnsDefinition { get; set; }

        public JsonDataTableFieldViewModel(JsonDataTableField field, string columnsDefinition) {
            Field = field;
            ColumnsDefinition = columnsDefinition;
        }
    }
}