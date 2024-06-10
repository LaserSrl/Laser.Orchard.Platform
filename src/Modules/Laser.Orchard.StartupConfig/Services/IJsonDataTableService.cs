using Laser.Orchard.StartupConfig.Models;
using Orchard;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IJsonDataTableService : IDependency {
        string ProcessColumnsDefinition(string columnsDefinition);
        string ParseTableDataForExport(JsonDataTableField field);
        string ParseTableDataForImport(JsonDataTableField field, string tableData, ImportContentContext context);
    }
}
