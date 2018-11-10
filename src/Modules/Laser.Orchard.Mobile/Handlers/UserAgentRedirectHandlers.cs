using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Mobile.Handlers {
    public class UserAgentRedirectHandler : ContentHandler {
        public UserAgentRedirectHandler(IRepository<UserAgentRedirectPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}