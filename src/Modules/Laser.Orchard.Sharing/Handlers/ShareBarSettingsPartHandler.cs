
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Laser.Orchard.Sharing.Models;

namespace Laser.Orchard.Sharing.Handlers
{
    
    public class ShareBarSettingsPartHandler : ContentHandler
    {
        public ShareBarSettingsPartHandler(IRepository<ShareBarSettingsPartRecord> repository)
        {
            Filters.Add(new ActivatingFilter<ShareBarSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
 }
    }
}