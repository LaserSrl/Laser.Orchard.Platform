using Laser.Orchard.FidelityGateway.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.FidelityGateway.Handlers
{
    public class FidelityUserHandler : ContentHandler
    {
        public FidelityUserHandler(IRepository<FidelityUserPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
