using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Extensions;
using Laser.Orchard.OpenAuthentication.Security;
using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.StartupConfig.IdentityProvider;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.Users.Events;
using System.Collections.Generic;
using System.Web.Mvc;


namespace Laser.Orchard.OpenAuthentication.Controllers {
    [Themed]
    public class AccountController : BaseAccountController {

        public AccountController(
            INotifier notifier,
            IOrchardOpenAuthWebSecurity orchardOpenAuthWebSecurity,
            IAuthenticationService authenticationService,
            IOpenAuthMembershipServices openAuthMembershipServices,
            IOrchardOpenAuthClientProvider openAuthClientProvider,
            IUserEventHandler userEventHandler,
            IUtilsServices utilsServices,
            IOrchardServices orchardServices,
            IEnumerable<IIdentityProvider> identityProviders) : base(
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
        public ContentResult ExternalTokenLogOn(string __provider__, string token, string secret = "",bool createPersistentCookie=false) {
            return ExternalTokenLogOnLogic(__provider__, token, secret, createPersistentCookie);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        [AlwaysAccessible]
        public ContentResult ExternalTokenLogOnSsl(string __provider__, string token, string secret = "",bool createPersistentCookie=false) {
            return ExternalTokenLogOnLogic(__provider__, token, secret, createPersistentCookie);
        }

    }

}