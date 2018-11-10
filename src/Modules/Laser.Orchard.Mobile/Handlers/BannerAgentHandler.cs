using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile.Handlers {
    [OrchardFeature("Laser.Orchard.BannerAgent")]
    public class BannerAgentHandler : ContentHandler {
        public BannerAgentHandler(IRepository<BannerAgentPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}