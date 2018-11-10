using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.StartupConfig.IdentityProvider;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Users.Events;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.OpenAuthentication.Controllers {
    public class AKAccountController : BaseAccountController {

        public AKAccountController(
            INotifier notifier,
            IUtilsServices utilsServices,
            IOrchardOpenAuthClientProvider openAuthClientProvider,
            IOrchardOpenAuthWebSecurity orchardOpenAuthWebSecurity,
            IEnumerable<IIdentityProvider> identityProviders,
            IOpenAuthMembershipServices openAuthMembershipServices,
            IAuthenticationService authenticationService,
            IUserEventHandler userEventHandler) : base(
                utilsServices,
                openAuthClientProvider,
                orchardOpenAuthWebSecurity,
                identityProviders,
                openAuthMembershipServices,
                authenticationService,
                userEventHandler,
                notifier
                ) { }

        [OutputCache(NoStore = true, Duration = 0)]
        [AlwaysAccessible]
        [WebApiKeyFilterForControllers(true)]
        public ContentResult ExternalTokenLogOn(string __provider__, string token, string secret = "",bool createPersistentCookie=false) {
            return ExternalTokenLogOnLogic(__provider__, token, secret, createPersistentCookie);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        [AlwaysAccessible]
        [WebApiKeyFilterForControllers(true)]
        public ContentResult ExternalTokenLogOnSsl(string __provider__, string token, string secret = "",bool createPersistentCookie=false) {
            return ExternalTokenLogOnLogic(__provider__, token, secret, createPersistentCookie);
        }

    }
}