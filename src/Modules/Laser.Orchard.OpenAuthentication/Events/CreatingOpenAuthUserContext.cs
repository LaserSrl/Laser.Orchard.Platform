using System.Collections.Generic;

namespace Laser.Orchard.OpenAuthentication.Events {
    public class CreatingOpenAuthUserContext : BaseOpenAuthUserContext {
        public CreatingOpenAuthUserContext(string userName, string emailAddress,
            string providerName, string providerUserId, IDictionary<string, string> extraData)
            : base(providerName, providerUserId, extraData) {
            UserName = userName;
            EmailAddress = emailAddress;
        }

        public string UserName { get; private set; }
        public string EmailAddress { get; set; }
    }
}