using Orchard.Security;
using System.Collections.Generic;

namespace Laser.Orchard.OpenAuthentication.Events {
    public class CreatedOpenAuthUserContext : BaseOpenAuthUserContext {
        public CreatedOpenAuthUserContext(IUser user, 
            string providerName, string providerUserId, IDictionary<string, string> extraData) 
            : base(providerName, providerUserId, extraData) {
            User = user;
        }

        public IUser User { get; private set; }
    }
}