using Laser.Orchard.StartupConfig.Security;
using Orchard;
using Orchard.Security;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IBearerTokenEncryptionService : IDependency {
        string CreateNewTokenForUser(IUser user);
        BearerTokenAuthenticationTicket CreateNewTicket(IUser user);
        string CreateTokenFromTicket(BearerTokenAuthenticationTicket ticket);
        BearerTokenAuthenticationTicket ParseToken(string token);
    }
}
