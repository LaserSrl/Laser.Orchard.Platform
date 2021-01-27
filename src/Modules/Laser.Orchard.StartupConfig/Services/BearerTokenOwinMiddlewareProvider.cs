using Laser.Orchard.StartupConfig.Security;
using Orchard.Environment.Extensions;
using Orchard.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Laser.Orchard.StartupConfig.Services {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class BearerTokenOwinMiddlewareProvider : IOwinMiddlewareProvider {
        private readonly IBearerTokenEncryptionService _bearerTokenEncryptionService;

        public BearerTokenOwinMiddlewareProvider(
            IBearerTokenEncryptionService bearerTokenEncryptionService) {

            _bearerTokenEncryptionService = bearerTokenEncryptionService;
        }

        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            return new[] {
                new OwinMiddlewareRegistration {
                    Configure = app =>
                        app.Use(async (context, next) => {
                            var authHeader = context.Request
                                .Headers
                                .Where(h => ("Authorization").Equals(h.Key, StringComparison.InvariantCultureIgnoreCase))
                                .SelectMany(h => h.Value)
                                .Select(s => s.Trim())
                                .Where(s => s.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase));
                            if (authHeader.Count() == 1) {
                                // If we have multiple bearer tokens in the call, we don't use any of them
                                // TODO: is this the right call? should we attempt to figure out a way to 
                                // pick the "correct" token?
                                // Extract token value
                                var token = authHeader.First()
                                    .Substring(7)
                                    .Trim();
                                // decrypt the token
                                var ticket = _bearerTokenEncryptionService.ParseToken(token);
                                if (ticket != null && !ticket.Expired) {
                                    // create an Identity for the Bearer Token
                                    var bearerIdentity =
                                        new BearerTokenIdentity(ticket);
                                    var user = (context.Authentication.User == null
                                        || !context.Authentication.User.Identity.IsAuthenticated)
                                        ? new ClaimsPrincipal()
                                        : new ClaimsPrincipal(context.Authentication.User);
                                    user.AddIdentity(bearerIdentity);
                                    context.Authentication.User = user;
                                }
                            }
                            await next.Invoke();
                        })
                }
            };
        }
    }
}