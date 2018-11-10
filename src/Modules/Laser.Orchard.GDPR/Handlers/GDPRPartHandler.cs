using Laser.Orchard.GDPR.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.GDPR.Handlers {
    public class GDPRPartHandler : ContentHandler {

        public GDPRPartHandler(IRepository<GDPRPartRecord> repository) {

            Filters.Add(StorageFilter.For(repository));
        }
    }
}