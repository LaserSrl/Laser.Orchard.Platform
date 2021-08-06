using Laser.Orchard.StartupConfig.Security;
using Orchard;
using Orchard.Security;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IBearerTokenEncryptionService : IDependency {
        /// <summary>
        /// Generate a new bearer token for the user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        string CreateNewTokenForUser(IUser user);
        /// <summary>
        /// Creates a new ticket to be used when generating a bearer token
        /// for the given user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        BearerTokenAuthenticationTicket CreateNewTicket(IUser user);
        /// <summary>
        /// Based on a ticket, generate the bearer token.
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        string CreateTokenFromTicket(BearerTokenAuthenticationTicket ticket);
        /// <summary>
        /// Based on a token, compute the corresponding ticket.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        BearerTokenAuthenticationTicket ParseToken(string token);
    }
}
