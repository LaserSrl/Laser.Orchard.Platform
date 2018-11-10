using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Newtonsoft.Json.Linq;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Environment.Extensions;
using Laser.Orchard.UsersExtensions.Services;
using Laser.Orchard.MultiStepAuthentication.Services;
using Laser.Orchard.MultiStepAuthentication.DataContracts;
using Orchard.Users.Models;
using Orchard.Security;
using Orchard.Users.Events;
using Laser.Orchard.StartupConfig.IdentityProvider;
using System.Web.Http.Results;
using System.Web.Mvc;
using Orchard.Logging;

namespace Laser.Orchard.MultiStepAuthentication.Controllers {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    [WebApiKeyFilter(true)]
    [OutputCache(NoStore = true, Duration = 0)]
    public class NonceLoginApiController : BaseMultiStepAccountApiController {

        private readonly IUtilsServices _utilsServices;
        private readonly IUsersExtensionsServices _usersExtensionsServices;
        private readonly INonceService _nonceService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IEnumerable<IIdentityProvider> _identityProviders;
    
        public NonceLoginApiController(
            IUtilsServices utilsServices,
            IUsersExtensionsServices usersExtensionsServices,
            INonceService nonceService,
            IAuthenticationService authenticationService,
            IUserEventHandler userEventHandler,
            IEnumerable<IIdentityProvider> identityProviders
           ) {
       
            _utilsServices = utilsServices;
            _usersExtensionsServices = usersExtensionsServices;
            _nonceService = nonceService;
            _authenticationService = authenticationService;
            _userEventHandler = userEventHandler;
            _identityProviders = identityProviders;
        }


        public Response Get(string mail) {
            var uuid = GetUUIDFromHeader();
            if (string.IsNullOrWhiteSpace(uuid)) {
                return _utilsServices.GetResponse(ResponseType.UnAuthorized);
            }

            var user = _usersExtensionsServices.GetUserByMail(mail);
            var flowToUse = FlowType.Website;
            if (user != null) {
                var data = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(uuid)) {
                    data.Add("uuid", uuid);// uuid used only by mobile channel
                    flowToUse = FlowType.App;
                }
                // create and send an OTP (this may overwrite any previous OTP for the user)
                _nonceService.SendNewOTP(user, data, DeliveryChannelType.Email, flowToUse);
                return _utilsServices.GetResponse(ResponseType.Success);
            }

            return _utilsServices.GetResponse(ResponseType.UnAuthorized);
        }

        public Response Post(JObject message) {
          
            var uuid = GetUUIDFromHeader();
            if (string.IsNullOrWhiteSpace(uuid)) {
                return  _utilsServices.GetResponse(ResponseType.UnAuthorized);
            }
            NonceLoginMessage msgObj;
            try {
                msgObj = message.ToObject<NonceLoginMessage>();
            } catch (Exception) {
                return _utilsServices.GetResponse(ResponseType.UnAuthorized);
            }

            if (msgObj != null) {
                var data = new Dictionary<string, string>();
                    data.Add("uuid", uuid);
                
                var iuser = _nonceService.UserFromNonce(msgObj.Nonce, data);
                if (iuser != null) {
                    // log the user in
                    var user = iuser as UserPart;
                    _authenticationService.SignIn(user, true);
                    _userEventHandler.LoggedIn(user);
                    return  _utilsServices.GetUserResponse("", _identityProviders);
                }
            }

            return _utilsServices.GetResponse(ResponseType.UnAuthorized);
        }

        private string GetUUIDFromHeader() {
            if (!Request.Headers.Contains("x-UUID"))
                return null;
            // get the device's uuid
            var uuid = string.Join("", Request.Headers
                .FirstOrDefault(head =>
                    head.Key.Equals("x-UUID", StringComparison.InvariantCultureIgnoreCase))
                .Value);
            return uuid;
        }

    }
}