﻿using Laser.Orchard.Mobile.ViewModels;
using Laser.Orchard.StartupConfig.IdentityProvider;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Laser.Orchard.UsersExtensions.Filters;
using Laser.Orchard.UsersExtensions.Models;
using Laser.Orchard.UsersExtensions.Providers;
using Laser.Orchard.UsersExtensions.Services;
using Orchard;
using Orchard.Mvc.AntiForgery;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Services;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;

namespace Laser.Orchard.UsersExtensions.Controllers {

    [OutputCache(NoStore = true, Duration = 0)]
    [RoutePrefix("AKUserActions")]
    public class AKUserActionsController : BaseUserActionsController {
        public AKUserActionsController(
            IOrchardServices orchardServices,
            IUsersExtensionsServices usersExtensionsServices,
            IUserService userService,
            IUtilsServices utilsServices,
            IUserEventHandler userEventHandler,
            IEnumerable<IIdentityProvider> identityProviders,
            ICsrfTokenHelper csrfTokenHelper,
            IMembershipService membershipService,
            IEnumerable<IExtendedRegistrationProvider> extendedRegistrationProviders
) : base(
                 orchardServices,
                 utilsServices,
                 usersExtensionsServices,
                 identityProviders,
                 userService,
                 userEventHandler,
                 csrfTokenHelper,
                 membershipService,
                 extendedRegistrationProviders
                 ) {
        }


        #region [https calls]

        /// <summary>
        /// </summary>
        /// <param name="userRegistrationParams">
        /// </param>
        /// <returns></returns>
        [HttpPost]
        [AlwaysAccessible]
        [WebApiKeyFilterForControllers(true)]
        public ContentResult RegisterSsl(UserRegistration userRegistrationParams) {
            return RegisterLogic(userRegistrationParams);
        }

        [HttpPost]
        [AlwaysAccessible]
        [WebApiKeyFilterForControllers(true)]
        public ContentResult SignInSsl(UserLogin login) {
            return SignInLogic(login);
        }

        [HttpPost]
        [WebApiKeyFilterForControllers(true)]
        [ValidateAntiForgeryTokenOrchard(false)]
        [PolicyApiFilter(Skip = true)] //Skip Policy Check
        public JsonResult SignOutSsl() {
            return SignOutLogic();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="phoneNumber">
        ///
        /// {
        ///     "phoneNumber":{
        ///         "internationalPrefix":"39",
        ///         "phoneNumber":"3477543903"
        ///     }
        /// }
        /// </param>
        /// <returns></returns>
        [HttpPost]
        [AlwaysAccessible]
        [WebApiKeyFilterForControllers(true)]
        public JsonResult RequestLostPasswordSmsSsl(PhoneNumberViewModel phoneNumber) {
            return RequestLostPasswordLogic(phoneNumber.PhoneNumber, LostPasswordUserOptions.Phone, phoneNumber.InternationalPrefix);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="username">
        ///  {
        ///      "username":"h.sbicego"
        ///  }
        /// </param>
        /// <returns></returns>
        [HttpPost]
        [AlwaysAccessible]
        [WebApiKeyFilterForControllers(true)]
        public JsonResult RequestLostPasswordAccountOrEmailSsl(string username) {
            return RequestLostPasswordLogic(username, LostPasswordUserOptions.Account);
        }

        [HttpGet]
        [AlwaysAccessible]
        [WebApiKeyFilterForControllers(true)]
        public ContentResult GetCleanRegistrationPoliciesSsl(string lang = null) {
            return GetCleanRegistrationPoliciesLogic(lang);
        }

        [HttpGet]
        [AlwaysAccessible]
        [WebApiKeyFilterForControllers(true)]
        public ContentResult GetRegistrationPoliciesSsl(string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, string lang = null) {
            return GetRegistrationPoliciesLogic(mfilter, page, pageSize, tinyResponse, minified, realformat, deeplevel, lang);
        }

        [HttpGet]
        [AlwaysAccessible]
        [WebApiKeyFilterForControllers(true)]
        public JsonResult GetUserRegistrationModelSsl() {
            return GetUserRegistrationModelLogic();
        }

        [HttpPost, AlwaysAccessible]
        public JsonResult ChallengeEmailApiSsl(string nonce) {
            return ChallengeEmailApiLogic(nonce);
        }

        [HttpPost]
        [AlwaysAccessible]
        [ValidateAntiForgeryTokenOrchard(false)]
        [WebApiKeyFilterForControllers(true)]
        public ContentResult ChangePasswordApiSsl(string currentPassword, string newPassword, string confirmPassword) {
            return ChangePasswordLogic(currentPassword, newPassword, confirmPassword);
        }

        [HttpPost]
        [AlwaysAccessible]
        [WebApiKeyFilterForControllers(true)]
        public ContentResult ChangeExpiredPasswordApiSsl(string currentPassword, string newPassword, string confirmPassword, string userName) {
            return ChangeExpiredPasswordLogic(currentPassword, newPassword, confirmPassword, userName);
        }

        [HttpPost]
        [AlwaysAccessible]
        [WebApiKeyFilterForControllers(true)]
        public ContentResult ChangeLostPasswordApiSsl(string nonce, string newPassword, string confirmPassword) {
            return ChangeLostPasswordLogic(nonce, newPassword, confirmPassword);
        }

        [HttpPost]
        [AlwaysAccessible]
        [WebApiKeyFilterForControllers(true)]
        public ContentResult SendChallengeEmailApiSsl(string username) {
            return SendChallengeEmailLogic(username);
        }

        #endregion [https calls]

    }
}