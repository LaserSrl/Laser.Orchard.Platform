using Laser.Orchard.DataProtection.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.DataProtection.Handlers {
    public class DataRestrictionsPartHandler : ContentHandler {
        public DataRestrictionsPartHandler(IRepository<DataRestrictionsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}