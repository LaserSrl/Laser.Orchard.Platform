using Laser.Orchard.StartupConfig.Helpers;
using Laser.Orchard.StartupConfig.Security;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Security.Providers;
using Orchard.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Services {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class BearerTokenAuthenticationService : FormsAuthenticationService, IAuthenticationService {
        // We inherit from FormsAuthenticationService so that we can correctly
        // provide the CurrentUser when they are authenticating with a token rather
        // than with the .ASPXAUTH cookie. We also replicate a lot of the logic from
        // that class when analysing an identity related to a bearer token.

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMembershipService _membershipService;
        private readonly IEnumerable<IBearerTokenDataProvider> _bearerTokenDataProviders;

        public BearerTokenAuthenticationService(
            ShellSettings settings,
            IClock clock,
            IMembershipService membershipService,
            IHttpContextAccessor httpContextAccessor,
            ISslSettingsProvider sslSettingsProvider,
            IMembershipValidationService membershipValidationService,
            IEnumerable<IUserDataProvider> userDataProviders,
            ISecurityService securityService,
            IEnumerable<IBearerTokenDataProvider> bearerTokenDataProviders) : 
            base(
                settings,
                clock,
                membershipService,
                httpContextAccessor,
                sslSettingsProvider,
                membershipValidationService,
                userDataProviders,
                securityService) {

            _httpContextAccessor = httpContextAccessor;
            _membershipService = membershipService;
            _bearerTokenDataProviders = bearerTokenDataProviders;
        }


        private IUser _signedInUser;
        private bool _isAuthenticated;
        // This fixes a performance issue when the forms authentication cookie is set to a
        // user name not mapped to an actual Orchard user content item. If the request is
        // authenticated but a null user is returned, multiple calls to GetAuthenticatedUser
        // will cause multiple DB invocations, slowing down the request. We therefore
        // remember if the current user is a non-Orchard user between invocations.
        private bool _isNonOrchardUser;

        public new IUser GetAuthenticatedUser() {

            if (_isNonOrchardUser)
                return null;
            if (_signedInUser != null || _isAuthenticated)
                return _signedInUser;

            // may be authenticated "normally" with .ASPXAUTH cookie
            _signedInUser = base.GetAuthenticatedUser();
            if (_signedInUser == null) {
                // logic to try to get an Orchard User based on the user from the bearer token
                var httpContext = _httpContextAccessor.Current();

                if (httpContext.IsBackgroundContext() 
                    || !httpContext.Request.IsAuthenticated 
                    || !(httpContext.User.Identity is BearerTokenIdentity)) {
                    return null;
                }
                // get info from identity
                var bearerIdentity = (BearerTokenIdentity)httpContext.User.Identity;
                var userData = bearerIdentity.Ticket.UserData ?? "";
                var userDataDictionary = new Dictionary<string, string>();
                try {
                    userDataDictionary = BearerTokenHelpers.DeserializeUserData(userData);
                } catch (Exception) {
                    return null;
                }
                // 1. Take the username
                if (!userDataDictionary.ContainsKey("UserName")) {
                    return null; // should never happen, unless the cookie has been tampered with
                }
                var userName = userDataDictionary["UserName"];
                _signedInUser = _membershipService.GetUser(userName);

                if (_signedInUser == null) {
                    _isNonOrchardUser = true;
                    return null;
                }

                // 2. Check the other stuff from the dictionary
                var validLogin = _bearerTokenDataProviders.All(udp => udp.IsValid(_signedInUser, userDataDictionary));
                if (!validLogin) {
                    _signedInUser = null;
                    return null;
                }

                _isAuthenticated = true;
            }
            return _signedInUser;
        }


    }
}