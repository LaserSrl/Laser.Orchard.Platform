using Laser.Orchard.DataProtection.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.DataProtection.Handlers {
    public class DataProtectionContextPartHandler : ContentHandler {
        public DataProtectionContextPartHandler(IRepository<DataProtectionContextPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}