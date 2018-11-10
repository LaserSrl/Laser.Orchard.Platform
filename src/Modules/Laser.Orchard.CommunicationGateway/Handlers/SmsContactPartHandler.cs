using Laser.Orchard.CommunicationGateway.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.CommunicationGateway.Handlers {

    public class SmsContactPartHandler : ContentHandler {

        public SmsContactPartHandler(IRepository<SmsContactPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}