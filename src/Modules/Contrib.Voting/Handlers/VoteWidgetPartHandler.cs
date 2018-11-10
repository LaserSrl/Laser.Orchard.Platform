using Contrib.Voting.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Contrib.Voting.Handlers {
    public class VoteWidgetPartHandler : ContentHandler {
        public VoteWidgetPartHandler(IRepository<VoteWidgetPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}