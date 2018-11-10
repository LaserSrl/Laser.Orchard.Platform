using Laser.Orchard.DynamicNavigation.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.DynamicNavigation.Handlers {

  public class DynamicMenuHandler : ContentHandler {

    public DynamicMenuHandler(IRepository<DynamicMenuRecord> repository) {
      Filters.Add(StorageFilter.For(repository));
    }
  }
}