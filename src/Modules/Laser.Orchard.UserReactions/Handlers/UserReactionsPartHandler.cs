using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.Services;
using Laser.Orchard.UserReactions.Drivers;


namespace Laser.Orchard.UserReactions.Handlers {
    public class UserReactionsPartHandler : ContentHandler {
        public UserReactionsPartHandler(IRepository<UserReactionsPartRecord> repository, IUserReactionsService reactionsService) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}