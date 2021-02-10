using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Extensions;
using Laser.Orchard.OpenAuthentication.Security;
using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.StartupConfig.Extensions;
using Laser.Orchard.StartupConfig.IdentityProvider;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Users.Events;
using System;
using System.Collections.Generic;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Orchard.Logging;
using Laser.Orchard.OpenAuthentication.ViewModels;

namespace Laser.Orchard.OpenAuthentication.Controllers {
    /// <summary>
    /// This abstract controller exists because we have to manage a situation where we would
    /// want all the logic in the actions to be under protection of an ApiKey filter, but the
    /// earlier versions of the mobile applications calling our APIs do not send that. Hence
    /// we have all the logic here, and then the implementation handles any eventual filter or
    /// layer that may be required on top.
    /// </summary>
    public abstract class BaseAccountController : Controller {

        private readonly IUtilsServices _utilsServices;
        private readonly IOrchardOpenAuthClientProvider _openAuthClientProvider;
        private readonly IOrchardOpenAuthWebSecurity _orchardOpenAuthWebSecurity;
        private readonly IEnumerable<IIdentityProvider> _identityProviders;
        private readonly IOpenAuthMembershipServices _openAuthMembershipServices;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly INotifier _notifier;

        public BaseAccountController(
            IUtilsServices utilsServices,
            IOrchardOpenAuthClientProvider openAuthClientProvider,
            IOrchardOpenAuthWebSecurity orchardOpenAuthWebSecurity,
            IEnumerable<IIdentityProvider> identityProviders,
            IOpenAuthMembershipServices openAuthMembershipServices,
            IAuthenticationService authenticationService,
            IUserEventHandler userEventHandler,
            INotifier notifier) {

            _utilsServices = utilsServices;
            _openAuthClientProvider = openAuthClientProvider;
            _orchardOpenAuthWebSecurity = orchardOpenAuthWebSecurity;
            _identityProviders = identityProviders;
            _openAuthMembershipServices = openAuthMembershipServices;
            _authenticationService = authenticationService;
            _userEventHandler = userEventHandler;
            _notifier = notifier;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [HttpPost]
        [AlwaysAccessible]
        public ActionResult ExternalLogOn(string providerName, string returnUrl) {
            // check if this post request has to be processed by ExternalLogOn httpget action
            var ctx = System.Web.HttpContext.Current;
            if ( ! ctx.Request.QueryString.ToString().Contains("__provider__")) {
                var stateString = HttpUtility.HtmlDecode(ctx.Request.Form["state"]);
                if (!string.IsNullOrWhiteSpace(stateString)) {
                    var q = HttpUtility.ParseQueryString(stateString);
                    if (string.IsNullOrWhiteSpace(returnUrl) && Array.IndexOf(q.AllKeys, "ReturnUrl") >= 0) {
                        returnUrl = q["ReturnUrl"];
                        return ExternalLogOn(returnUrl);
                    }
                }
            }
            return new OpenAuthLoginResult(providerName, Url.OpenAuthLogOn(returnUrl));
        }

        [AlwaysAccessible]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult ExternalLogOn(string returnUrl, bool createPersistentCookie=false) {
            AuthenticationResult result = _orchardOpenAuthWebSecurity.VerifyAuthentication(Url.OpenAuthLogOn(returnUrl));

            if (!result.IsSuccessful) {
                _notifier.Error(T("Your authentication request failed."));
                return new RedirectResult(Url.LogOn(returnUrl));
            }

            if (_orchardOpenAuthWebSecurity.Login(result.Provider, result.ProviderUserId)) {
                return WebSignInSuccessful(result.Provider, returnUrl);
            }

            // At this point, login using the OpenAuth provider failed, meaning that we could not find a match
            // between the information from the provider and Orchard's users.

            // Get additional UserData
            if (result.ExtraData.ContainsKey("accesstoken")) {
                result = _openAuthClientProvider.GetUserData(result.Provider, result, result.ExtraData["accesstoken"]);
            } else {
                result = _openAuthClientProvider.GetUserData(result.Provider, result, "");
            }
            // _openAuthClientProvider.GetUserData(params) may return null if there is no configuration for a provider
            // with the given name.
            if (result == null) {
                // handle this condition and exit the method
                _notifier.Error(T("Your authentication request failed."));
                return new RedirectResult(Url.LogOn(returnUrl));
            }
            
            // In what condition can GetAuthenticatedUser() not be null? To reach this code, _orchardOpenAuthWebSecurity.Login(params)
            // must have returned false. That happens if there was no record for the combination Provider/ProviderUserId, or if
            // GetAuthenticatedUser() returned null in it. In the latter case, it should still return null. The former case means
            // we are trying to login with an OAuth provider and it's the first time we are calling it for this user, but we are 
            // also already authenticated in some other way. This only makes sense in a situation where, as authenticated users,
            // we are allowed to add information from OAuth providers to our account: Users/Account/LogOn, if the user is authenticated,
            // redirects to the homepage, and does not give an option to go and login again using OAuth.

            // The authenticated User or depending from settings the first created user with the same e-mail
            var masterUser = GetMasterUser(result);

            if (masterUser != null) {
                return WebUserMergeAndSignIn(masterUser, result, returnUrl, createPersistentCookie);
            }

            // here masterUser is null
            if (_openAuthMembershipServices.CanRegister()) {
                // User can register and there is not a user with the same email
                return WebOAuthRegister(result, returnUrl, createPersistentCookie);
            }

            // We are in the case which we cannot create new accounts, we have no user to merge, the user is not logged in
            // so we ask to user to login to merge accounts
            string loginData = _orchardOpenAuthWebSecurity.SerializeProviderUserId(result.Provider,
                                                                                       result.ProviderUserId);

            ViewBag.ProviderDisplayName = _orchardOpenAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;

            // the LogOn Helper here is not doing any validaiton on the stuff it's putting in query string, so it 
            // may end up having forbidden character sequences.
            return new RedirectResult(Url.LogOn(returnUrl, result.UserName, loginData));
        }

        [OutputCache(NoStore = true, Duration = 0)]
        protected ContentResult ExternalTokenLogOnLogic(string __provider__, string token, string secret = "", bool createPersistentCookie=false) {
            // TempDataDictionary registeredServicesData = new TempDataDictionary();
            var result = new Response();

            try {
                if (string.IsNullOrWhiteSpace(__provider__) || string.IsNullOrWhiteSpace(token)) {
                    result = _utilsServices.GetResponse(ResponseType.None, "One or more of the required parameters was not provided or was an empty string.");
                    return _utilsServices.ConvertToJsonResult(result);
                }

                // ricava il return URL così come registrato nella configurazione del provider di OAuth (es. Google)
                var returnUrl = Url.MakeAbsolute(Url.Action("ExternalLogOn", "Account"));
                AuthenticationResult authResult = ResultForProvider(__provider__, token, secret, returnUrl);
                // authResult may be null if the provider name matches no configured provider
                if (authResult == null || !authResult.IsSuccessful) {
                    result = _utilsServices.GetResponse(ResponseType.InvalidUser, "Token authentication failed.");
                    return _utilsServices.ConvertToJsonResult(result);
                } else {

                    if (_orchardOpenAuthWebSecurity.Login(authResult.Provider, authResult.ProviderUserId, createPersistentCookie)) {
                        // Login also returns false for disabled users (this used to not be the case)
                        if (HttpContext.Response.Cookies.Count == 0) {
                            // For some reason, SignIn failed to add the authentication cookie to the response
                            result = _utilsServices.GetResponse(ResponseType.None, "Unable to send back a cookie.");
                            return _utilsServices.ConvertToJsonResult(result);
                        } else {
                            // The LoggedIn event is already raised in the Login method just before returning true, 
                            // so we should not be raising it here as well.
                            return _utilsServices.ConvertToJsonResult(_utilsServices.GetUserResponse("", _identityProviders));
                        }
                    }
                    // Login returned false: either the user given by Provider+UserId has never been registered (so we have no
                    // matching username to use), or no user exists in Orchard with that username, or SignIn failed somehow, or
                    // the user is disabled.

                    // This is an attempt to login using an OAuth provider. The call to .Login(params) returned false. In an actual 
                    // login there is no reason why GetAuthenticatedUser() should return a user, unless we are in a situation where,
                    // as authenticated users, we are allowed to add information from OAuth providers to our account

                    // The authenticated User or depending from settings the first created user with the same e-mail
                    var masterUser = GetMasterUser(authResult);
                    
                    if (masterUser != null) {
                        return MobileUserMergeAndSignIn(masterUser, authResult, returnUrl, createPersistentCookie);
                    }

                    // here masterUser is null
                    if (_openAuthMembershipServices.CanRegister()) {
                        // User can register and there is not a user with the same email
                        return MobileOAuthRegister(authResult, returnUrl, createPersistentCookie);
                    }

                    result = _utilsServices.GetResponse(ResponseType.None, "Login failed.");
                    return _utilsServices.ConvertToJsonResult(result);
                }
            } catch (Exception e) {
                result = _utilsServices.GetResponse(ResponseType.None, e.Message);
                return _utilsServices.ConvertToJsonResult(result);
            }
        }

        /// <summary>
        /// This method is designed to handle cases where we have the oauth token to verify, but we did
        /// not request it directly server side (just as is the case for oauth through mobile sdks) but
        /// we want to respond with a web page (as we do with oauth from a web browser). An example case
        /// for this is authenticating with an external provider within an iframe, where the "outer" app
        /// is handling most of the steps and providing us with a token to verify. We cannot use the 
        /// "normal" procedure we have in the ExternalLogOn action to verify this token, because the steps
        /// there have provisions to prevent CORS.
        /// </summary>
        /// <param name="__provider__"></param>
        /// <param name="token"></param>
        /// <param name="secret"></param>
        /// <param name="createPersistentCookie"></param>
        /// <returns></returns>
        [AlwaysAccessible]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult ExternalTokenLogonWeb(
            string __provider__, string token, string secret = "", bool createPersistentCookie = false, string returnUrl = "") {

            try {
                // required parameters
                if (string.IsNullOrWhiteSpace(__provider__) || string.IsNullOrWhiteSpace(token)) {
                    return AuthenticationFailed(Url.LogOn(returnUrl));
                }
                
                AuthenticationResult authResult = ResultForProvider(__provider__, token, secret, returnUrl);
                // authResult may be null if the provider name matches no configured provider
                if (authResult == null || !authResult.IsSuccessful) {
                    return AuthenticationFailed(Url.LogOn(returnUrl));
                } else {
                    if (_orchardOpenAuthWebSecurity.Login(authResult.Provider, authResult.ProviderUserId, createPersistentCookie)) {
                        // Login also returns false for disabled users (this used to not be the case)
                        if (HttpContext.Response.Cookies.Count == 0) {
                            // For some reason, SignIn failed to add the authentication cookie to the response
                            return AuthenticationFailed(Url.LogOn(returnUrl));
                        } else {
                            // The LoggedIn event is already raised in the Login method just before returning true, 
                            // so we should not be raising it here as well.
                            return WebSignInSuccessful(authResult.Provider, returnUrl);
                        }
                    }
                    // Login returned false: either the user given by Provider+UserId has never been registered (so we have no
                    // matching username to use), or no user exists in Orchard with that username, or SignIn failed somehow, or
                    // the user is disabled.

                    // The authenticated User or depending from settings the first created user with the same e-mail
                    var masterUser = GetMasterUser(authResult);

                    if (masterUser != null) {
                        return WebUserMergeAndSignIn(masterUser, authResult, returnUrl, createPersistentCookie);
                    }

                    // here masterUser is null
                    if (_openAuthMembershipServices.CanRegister()) {
                        // User can register and there is not a user with the same email
                        return WebOAuthRegister(authResult, returnUrl, createPersistentCookie);
                    }
                }
            } catch (Exception ex) {
                Logger.Error(ex.Message);
            }
            return AuthenticationFailed(returnUrl);
        }

        private AuthenticationResult ResultForProvider(
            string __provider__, string token, string secret = "", string returnUrl = "") {
            AuthenticationResult dummy = new AuthenticationResult(true);
            return _openAuthClientProvider.GetUserData(__provider__, dummy, token, secret, returnUrl);
        }
        private ActionResult AuthenticationFailed(string returnUrl) {
            _notifier.Error(T("Your authentication request failed."));
            return this.RedirectLocal(returnUrl, Url.LogOn(returnUrl));
        }
        private ActionResult WebSignInSuccessful(string provider, string returnUrl) {
            return WebSignInSuccessful(T("You have been logged using your {0} account.", provider), returnUrl);
        }
        private ActionResult WebSignInSuccessful(LocalizedString message, string returnUrl) {
            _notifier.Information(message);

            return this.RedirectLocal(returnUrl);
        }
        private OpenAuthTemporaryUser CreateTemporaryUser(AuthenticationResult result) {
            // _openAuthClientProvider.NormalizeData(params) may return null if there is no configuration for a provider
            // with the given name. If result != null, that is not the case, because in that condition GetUserData(params)
            // would return null, and we would have already exited the method.
            var userParams = _openAuthClientProvider.NormalizeData(result.Provider, new OpenAuthCreateUserParams(result.UserName,
                                                        result.Provider,
                                                        result.ProviderUserId,
                                                        result.ExtraData));

            return _openAuthMembershipServices.CreateTemporaryUser(userParams);
        }
        // The authenticated User or depending from settings the first created user with the same e-mail
        private IUser GetMasterUser(AuthenticationResult result) {
            var masterUser = _authenticationService.GetAuthenticatedUser();
            if (masterUser == null) {
                var temporaryUser = CreateTemporaryUser(result);
                masterUser = _orchardOpenAuthWebSecurity.GetClosestMergeableKnownUser(temporaryUser);
            }
            return masterUser;
        }
        private ActionResult WebUserMergeAndSignIn(
            IUser masterUser, AuthenticationResult result,string returnUrl, bool createPersistentCookie = false) {

            UserMergeAndSignIn(masterUser, result, returnUrl, createPersistentCookie);

            return WebSignInSuccessful(result.Provider, returnUrl);
        }
        private ContentResult MobileUserMergeAndSignIn(
            IUser masterUser, AuthenticationResult result, string returnUrl, bool createPersistentCookie = false) {

            UserMergeAndSignIn(masterUser, result, returnUrl, createPersistentCookie);

            return _utilsServices
                .ConvertToJsonResult(_utilsServices
                    .GetUserResponse(T("Your {0} account has been attached to your local account.", result.Provider).Text,
                    _identityProviders));
        }
        private void UserMergeAndSignIn(
            IUser masterUser, AuthenticationResult result, string returnUrl, bool createPersistentCookie = false) {
            // If the current user is logged in or settings ask for a user merge and we found a User with the same email
            // create or merge accounts
            _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId,
                                                              masterUser, result.ExtraData);

            _notifier.Information(T("Your {0} account has been attached to your local account.", result.Provider));

            // Handle LoggedIn Event
            var authenticatedUser = _authenticationService.GetAuthenticatedUser();
            if (authenticatedUser == null) {
                _authenticationService.SignIn(masterUser, createPersistentCookie);
                _userEventHandler.LoggedIn(masterUser);
                // The LoggedIn event is invoked here, because if authenticateUser != null, then it means the user
                // had already logged in some other time
            }
        }
        private ActionResult WebOAuthRegister(
            AuthenticationResult result, string returnUrl, bool createPersistentCookie = false) {

            return OAuthRegister(result, returnUrl,
                (ar, us) => {
                    _userEventHandler.LoggedIn(us);
                    return WebSignInSuccessful(
                        T("You have been logged in using your {0} account. We have created a local account for you with the name '{1}'", ar.Provider, us.UserName),
                        returnUrl);
                },
                (ar, us) => AuthenticationFailed(returnUrl),
                createPersistentCookie);
        }
        private ContentResult MobileOAuthRegister(
            AuthenticationResult authResult, string returnUrl, bool createPersistentCookie = false) {
            
            return (ContentResult)OAuthRegister(authResult, returnUrl,
                (ar, us) => {
                    if (HttpContext.Response.Cookies.Count == 0) {
                        // SignIn adds the authentication cookie to the response, so that is what we are checking here
                        // We should never be here executing this code.
                        return _utilsServices.ConvertToJsonResult(
                            _utilsServices.GetResponse(ResponseType.None, "Unable to send back a cookie."));
                    } else {
                        // Handle LoggedIn Event
                        _userEventHandler.LoggedIn(us);
                        return _utilsServices.ConvertToJsonResult(
                            _utilsServices.GetUserResponse(
                                T("You have been logged in using your {0} account. We have created a local account for you with the name '{1}'",
                                    ar.Provider, us.UserName).Text,
                                _identityProviders));
                    }
                },
                (ar, us) => _utilsServices.ConvertToJsonResult(_utilsServices.GetResponse(ResponseType.None, "Login failed.")),
                createPersistentCookie);
        }
        private ActionResult OAuthRegister(
             AuthenticationResult result, string returnUrl,
             Func<AuthenticationResult, IUser, ActionResult> successDelegate,
             Func<AuthenticationResult, IUser, ActionResult> failDelegate, 
             bool createPersistentCookie = false) {

            var createUserParams = new OpenAuthCreateUserParams(result.UserName,
                            result.Provider,
                            result.ProviderUserId,
                            result.ExtraData);
            createUserParams = _openAuthClientProvider.NormalizeData(result.Provider, createUserParams);
            // Creating the user here calls the IMembershipService, that will take care of invoking the user events
            var newUser = _openAuthMembershipServices.CreateUser(createUserParams);
            // newUser may be null here, if creation of a new user fails.
            // TODO: we should elsewhere add an UserEventHandler that in the Creating event handles the case where
            // here we are trying to create a user with the same Username or Email as an existing one. That would simply
            // use IUserService.VerifyUnicity(username, email)
            if (newUser != null) {
                // CreateOrUpdateAccount causes specific OpenAuth events to fire
                _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(result.Provider,
                   result.ProviderUserId,
                   newUser,
                   result.ExtraData);
                // The default implementation of IOpendAuthMembershipService creates an approved user.
                // The events specific to open auth give points to attach handlers where the UserProviderRecord 
                // is populated correctly.

                _authenticationService.SignIn(newUser, createPersistentCookie);
                return successDelegate(result, newUser);
            }
            // if newUser == null, just go ahead and return the "Login Failed" Response
            return failDelegate(result, newUser);
        }
    }

    internal class OpenAuthLoginResult : ActionResult {
        private readonly string _providerName;
        private readonly string _returnUrl;

        public OpenAuthLoginResult(string providerName, string returnUrl) {
            _providerName = providerName;
            _returnUrl = returnUrl;
        }

        public override void ExecuteResult(ControllerContext context) {
            using (new TransactionScope(TransactionScopeOption.Suppress)) {
                var httpContext = HttpContext.Current;
                var securityManagerWrapper = httpContext.Request.RequestContext.GetWorkContext().Resolve<IOpenAuthSecurityManagerWrapper>();
                securityManagerWrapper.RequestAuthentication(_providerName, _returnUrl);
            }
        }
    }

}