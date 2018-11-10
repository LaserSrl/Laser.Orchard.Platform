using Laser.Orchard.Highlights.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Highlights.Handlers {

  public class HighlightsItemHandler : ContentHandler {

    public HighlightsItemHandler(IRepository<HighlightsItemPartRecord> repository) {
      Filters.Add(StorageFilter.For(repository));
    }
  }
}