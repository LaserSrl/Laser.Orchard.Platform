using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile.Handlers {

    [OrchardFeature("Laser.Orchard.Sms")]
    public class UserPwdRecoveryPartHandler : ContentHandler {
        public UserPwdRecoveryPartHandler(IRepository<UserPwdRecoveryPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}