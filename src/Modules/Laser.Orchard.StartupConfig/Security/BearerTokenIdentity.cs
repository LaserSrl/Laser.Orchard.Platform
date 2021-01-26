using Orchard.Environment.Extensions;
using System.Security.Claims;

namespace Laser.Orchard.StartupConfig.Security {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class BearerTokenIdentity : ClaimsIdentity {
        protected BearerTokenIdentity() : base("Bearer") { }

        public BearerTokenIdentity(BearerTokenAuthenticationTicket ticket) : this() {

            Ticket = ticket;
            this.AddClaim(new Claim(NameClaimType, ticket.Name));
        }

        public BearerTokenAuthenticationTicket Ticket { get; private set; }
    }
}