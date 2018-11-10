
using Laser.Orchard.OpenAuthentication.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.OpenAuthentication.Handlers {
    
    public class OpenAuthenticationSettingsPartHandler : ContentHandler {
        public OpenAuthenticationSettingsPartHandler(IRepository<OpenAuthenticationSettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<OpenAuthenticationSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}