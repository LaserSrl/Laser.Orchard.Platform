using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Services.Clients;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Models;
using System.Collections.Generic;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IOpenAuthSecurityManagerWrapper : IDependency {
        bool Login(string providerUserId, bool createPersistentCookie);
        AuthenticationResult VerifyAuthentication(string returnUrl);
        void RequestAuthentication(string providerName, string returnUrl);
    }

    public class OpenAuthSecurityManagerWrapper : IOpenAuthSecurityManagerWrapper {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrchardOpenAuthClientProvider _orchardOpenAuthClientProvider;
        private readonly IOrchardOpenAuthDataProvider _orchardOpenAuthDataProvider;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IEnumerable<IExternalAuthenticationClient> _openAuthAuthenticationClients;

        public OpenAuthSecurityManagerWrapper(IHttpContextAccessor httpContextAccessor,
                                              IOrchardOpenAuthClientProvider orchardOpenAuthClientProvider,
                                              IOrchardOpenAuthDataProvider orchardOpenAuthDataProvider,
                                              IAuthenticationService authenticationService,
                                              IMembershipService membershipService,
                                              IUserEventHandler userEventHandler,
                                              IEnumerable<IExternalAuthenticationClient> openAuthAuthenticationClients) {
            _httpContextAccessor = httpContextAccessor;
            _orchardOpenAuthClientProvider = orchardOpenAuthClientProvider;
            _orchardOpenAuthDataProvider = orchardOpenAuthDataProvider;
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _userEventHandler = userEventHandler;
            _openAuthAuthenticationClients = openAuthAuthenticationClients;
        }

        private string ProviderName {
            get { return OpenAuthSecurityManager.GetProviderName(_httpContextAccessor.Current()); }
        }



        public bool Login(string providerUserId, bool createPersistentCookie) {
            string userName = _orchardOpenAuthDataProvider.GetUserNameFromOpenAuth(ProviderName, providerUserId);

            if (string.IsNullOrWhiteSpace(userName))
                return false;

            // Orchard's "normal" LogOn process would have the LoggingIn event raised here
            // The parameters for that are Username and Password. Here we do not have the latter

            // Before SignIn, the normal Orchard process attempts to validate the information. In
            // case the validation fails, it raises the LoginFailed event. That has the same parameters
            // as the LoggingIn event, and we still don't have the password to pass along.

            // If GetUser(userName) != null, and we perform SignIn, the next call should in general return the
            // same IUser returned by GetUser(userName), unless that user has been disabled or some other way
            // disallowed from actually signing in. Note that, using the default Orchard.Users IMembershipService
            // along with the default FormsAuthenticationService, neither GetUser(userName) nor SignIn(user, flag)
            // validate that condition: as a consequence, here we could be signing in a disabled user.
            var user = _membershipService.GetUser(userName);
            // if the user is valid, sign them in
            if (user != null) {
                // Since this feature depends on Orchard.Users, we are allowed to use the same definition of "Approved"
                // user that is used there.
                var userPart = user.As<UserPart>();
                if (userPart != null 
                    && userPart.EmailStatus == UserStatus.Approved 
                    && userPart.RegistrationStatus == UserStatus.Approved) {
                    _authenticationService.SignIn(user, createPersistentCookie);
                }
            }

            // If we have done the SignIn above, this will return user. Otherwise it should return null, unless we
            // are in a weird condition where we are already authenticated and are trying to "add" an OAuth authentication.
            var authenticatedUser = _authenticationService.GetAuthenticatedUser();

            if (authenticatedUser == null) {

                return false;
            } else {
                // Login was successful using the OAuth provider, so we can fire the LoggedIn event
                _userEventHandler.LoggedIn(authenticatedUser);
                return true;
            }
        }

        public AuthenticationResult VerifyAuthentication(string returnUrl) {
            if (string.IsNullOrEmpty(ProviderName)) {
                // se non è noto il provider richiama il RewriteRequest di tutti i client registrati
                RewriteRequest();
            }
            if (string.IsNullOrEmpty(ProviderName)) {
                return AuthenticationResult.Failed;
            }
            var manager = SecurityManager(ProviderName);
            return manager.VerifyAuthentication(returnUrl);
        }


        public void RequestAuthentication(string providerName, string returnUrl) {
            SecurityManager(providerName).RequestAuthentication(returnUrl);
        }

        private OpenAuthSecurityManager SecurityManager(string providerName) {
            return new OpenAuthSecurityManager(_httpContextAccessor.Current(), _orchardOpenAuthClientProvider.GetClient(providerName), _orchardOpenAuthDataProvider);
        }
        private void RewriteRequest() {
            foreach (var client in _openAuthAuthenticationClients) {
                if (client.RewriteRequest()) {
                    break;
                }
            }
        }
    }
}