using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Newtonsoft.Json;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Laser.Orchard.StartupConfig.Controllers {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    [RoutePrefix("BearerToken")]
    public class BearerTokenController : ApiController {
        private readonly IBearerTokenEncryptionService _bearerTokenEncryptionService;
        private readonly IApiCredentialsManagementService _apiCredentialsManagementService;

        public BearerTokenController(
            IBearerTokenEncryptionService bearerTokenEncryptionService,
            IApiCredentialsManagementService apiCredentialsManagementService) {

            _bearerTokenEncryptionService = bearerTokenEncryptionService;
            _apiCredentialsManagementService = apiCredentialsManagementService;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        [HttpPost]
        [AlwaysAccessible]
        [ActionName("Auth")]
        public BearerTokenCreatedRespose Auth(BearerTokenAuthRequest credentials) {

            // Steps replicating what is in AccountController.LogOn
            // 1. LoggingIn events
            //   These tell the system that there is a login attempt
            //   _userEventHandler.LoggingIn(userNameOrEmail, password);

            // 2. Credentials validation
            //   var user = ValidateLogOn(userNameOrEmail, password);
            //   2.a "Formal" validation of the credentials
            //   2.b Fetch the user from db
            //   2.c Verify that the user may login
            //     var user = _membershipService.ValidateUser(userNameOrEmail, password);
            //   LoginFailed events
            //     _userEventHandler.LogInFailed(userNameOrEmail, password);
            var user = _apiCredentialsManagementService.ValidateCredentials(credentials.ApiKey, credentials.ApiSecret);
            if (user == null) {
                // unauthorized 
                var msg = new HttpResponseMessage(HttpStatusCode.Unauthorized) {
                    ReasonPhrase = T("Invalid credentials.").Text,
                    Content = new StringContent(
                        JsonConvert.SerializeObject(new BearerTokenApiResponseObject() {
                            Result = "NOT_OK",
                            ResultMessage = T("Invalid credentials.").Text
                        }).ToString())
                };
                msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                // We have to throw this exception for the ApiController to properly
                // "return" a 401 Unauthorized response.
                throw new HttpResponseException(msg);
            }
            // 3. Test for expired password
            //   TODO: set up a system for credentials expiration

            // 4. User SignIn
            //   Create the token we should return
            //   _authenticationService.SignIn(user, rememberMe);
            var token = _bearerTokenEncryptionService.CreateNewTokenForUser(user);

            // 5. LoggedIn events
            //   _userEventHandler.LoggedIn(user);

            // 6. Return the results
            return new BearerTokenCreatedRespose {
                Token = token
            };
        }

    }
}