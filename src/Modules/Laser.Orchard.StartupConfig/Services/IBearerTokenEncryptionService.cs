using Laser.Orchard.StartupConfig.Security;
using Orchard;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IBearerTokenEncryptionService : IDependency {
        string CreateTokenFromTicket(BearerTokenAuthenticationTicket ticket);
        BearerTokenAuthenticationTicket ParseToken(string token);
    }
}
