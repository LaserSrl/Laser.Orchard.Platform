using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class GTMProductHandler : ContentHandler {

        public GTMProductHandler(
            IRepository<GTMProductPartRecord> repository) {

            Filters.Add(StorageFilter.For(repository));
        }
    }
}
