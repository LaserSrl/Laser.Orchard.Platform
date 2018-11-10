using Laser.Orchard.CommunicationGateway.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.CommunicationGateway.Handlers {

    public class EmailContactPartHandler : ContentHandler {

        public EmailContactPartHandler(IRepository<EmailContactPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}