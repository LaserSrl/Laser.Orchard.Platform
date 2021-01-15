using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Environment.Extensions;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.StartupConfig.Controllers {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    [RoutePrefix("BearerToken")]
    public class BearerTokenController : ApiController {
        private readonly IBearerTokenEncryptionService _bearerTokenEncryptionService;

        public BearerTokenController(
            IBearerTokenEncryptionService bearerTokenEncryptionService) {
            _bearerTokenEncryptionService = bearerTokenEncryptionService;
        }

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

            // 3. Test for expired password
            //   TODO: set up a system for credentials expiration

            // 4. User SignIn
            //   Create the token we should return
            //   _authenticationService.SignIn(user, rememberMe);
            //var token = _bearerTokenEncryptionService.CreateNewTokenForUser(user);

            // 5. LoggedIn events
            //   _userEventHandler.LoggedIn(user);

            // 6. Return the results
            return new BearerTokenCreatedRespose {
                Token = ""//token
            };
        }

    }
}