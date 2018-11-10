using Laser.Orchard.Maps.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Maps.Handlers
{
    public class MapPartHandler : ContentHandler
    {
        public MapPartHandler(IRepository<MapVersionRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}