using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Handlers {
    [OrchardFeature("Laser.Orchard.StartupConfig.JsonDataTablePart")]
    public class JsonDataTablePartHandler : ContentHandler {
        public JsonDataTablePartHandler(IRepository<JsonDataTablePartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}