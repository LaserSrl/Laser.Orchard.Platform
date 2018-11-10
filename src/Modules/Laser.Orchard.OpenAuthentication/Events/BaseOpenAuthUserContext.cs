using System.Collections.Generic;

namespace Laser.Orchard.OpenAuthentication.Events {
    public abstract class BaseOpenAuthUserContext {

        public BaseOpenAuthUserContext(string providerName, string providerUserId, IDictionary<string, string> extraData) {
            ProviderName = providerName;
            ProviderUserId = providerUserId;
            ExtraData = extraData;
        }

        public virtual string ProviderName { get; private set; }
        public virtual string ProviderUserId { get; private set; }
        public virtual IDictionary<string, string> ExtraData { get; private set; }
    }
}