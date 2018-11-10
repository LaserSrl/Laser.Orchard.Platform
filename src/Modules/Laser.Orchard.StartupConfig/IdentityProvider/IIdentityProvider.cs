using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.IdentityProvider {
    public interface IIdentityProvider : IDependency {
        KeyValuePair<string, object> GetRelatedId(Dictionary<string, object> context);
    }
}
