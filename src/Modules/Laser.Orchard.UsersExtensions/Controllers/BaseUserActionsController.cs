using Laser.Orchard.Commons.Services;
using Laser.Orchard.Mobile.ViewModels;
using Laser.Orchard.StartupConfig.IdentityProvider;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.UsersExtensions.Filters;
using Laser.Orchard.UsersExtensions.Models;
using Laser.Orchard.UsersExtensions.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Laser.Orchard.UsersExtensions.Controllers {
    /// <summary>
    /// This abstract controller exists because we have to manage a situation where we would
    /// want all the logic in the actions to be under protection of an ApiKey filter, but the
    /// earlier versions of the mobile applications calling our APIs do not send that. Hence
    /// we have all the logic here, and then the implementation handles any eventual filter or
    /// layer that may be required on top.
    /// </summary>
    public abstract class BaseUserActionsController : Controller {

        private readonly IUsersExtensionsServices _usersExtensionsServices;
        private readonly IEnumerable<IIdentityProvider> _identityProviders;
        private readonly IUserService _userService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IMembershipService _membershipService;

        public BaseUserActionsController(
            IOrchardServices orchardServices,
            IUtilsServices utilsServices,
            IUsersExtensionsServices usersExtensionsServices,
            IEnumerable<IIdentityProvider> identityProviders,
            IUserService userService,
            IUserEventHandler userEventHandler,
            ICsrfTokenHelper csrfTokenHelper,
            IMembershipService membershipService) {
            OrchardServices = orchardServices;
            UtilsServices = utilsServices;
            _usersExtensionsServices = usersExtensionsServices;
            _identityProviders = identityProviders;
            _userService = userService;
            _userEventHandler = userEventHandler;
            CsrfTokenHelper = csrfTokenHelper;
            _membershipService = membershipService;
            T = NullLocalizer.Instance;
        }

        protected BaseUserActionsController(IOrchardServices orchardServices, IUtilsServices utilsServices, IUsersExtensionsServices usersExtensionsServices, IEnumerable<IIdentityProvider> identityProviders, IUserService userService, IUserEventHandler userEventHandler) {
            OrchardServices = orchardServices;
            UtilsServices = utilsServices;
        }

        #region [http calls]

        [HttpPost]
        public ContentResult Register(UserRegistration userRegistrationParams) {
            return RegisterLogic(userRegistrationParams);
        }

        [HttpPost]
        public ContentResult SignIn(UserLogin login) {
            return SignInLogic(login);
        }

        [HttpPost]
        public JsonResult SignOut() {
            return SignOutLogic();
        }

        [HttpPost]
        public JsonResult RequestLostPasswordSms(PhoneNumberViewModel phoneNumber) {
            return RequestLostPasswordLogic(phoneNumber.PhoneNumber, LostPasswordUserOptions.Phone, phoneNumber.InternationalPrefix);
        }

        [HttpPost]
        public JsonResult RequestLostPasswordAccountOrEmail(string username) {
            return RequestLostPasswordLogic(username, LostPasswordUserOptions.Account);
        }

        [HttpGet]
        public ContentResult GetCleanRegistrationPolicies(string lang = null) {
            return GetCleanRegistrationPoliciesLogic(lang);
        }

        [HttpGet]
        public ContentResult GetRegistrationPolicies(string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, string lang = null) {
            return GetRegistrationPoliciesLogic(mfilter, page, pageSize, tinyResponse, minified, realformat, deeplevel, lang);
        }

        [HttpGet]
        public JsonResult GetUserRegistrationModel() {
            return GetUserRegistrationModelLogic();
        }

        [HttpPost]
        public ContentResult ChangePassword(string currentPassword, string newPassword, string confirmPassword) {
            return ChangePasswordLogic(currentPassword, newPassword, confirmPassword);
        }

        [HttpPost]
        public ContentResult ChangeExpiredPassword(string currentPassword, string newPassword, string confirmPassword, string userName) {
            return ChangeExpiredPasswordLogic(currentPassword, newPassword, confirmPassword, userName);
        }

        [HttpPost]
        public ContentResult ChangeLostPassword(string nonce, string newPassword, string confirmPassword) {
            return ChangeLostPasswordLogic(nonce, newPassword, confirmPassword);
        }

        [HttpPost]
        public ContentResult SendChallengeEmail(string username) {
            return SendChallengeEmailLogic(username);
        }

        #endregion [http calls]

        public Localizer T { get; set; }
        public IOrchardServices OrchardServices { get; }
        public IUtilsServices UtilsServices { get; }
        public ICsrfTokenHelper CsrfTokenHelper { get; }

        protected ContentResult RegisterLogic(UserRegistration userRegistrationParams) {
            Response result;
            ResponseType responseType;
            // ensure users can request lost password
            var registrationSettings = OrchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
            if (!registrationSettings.UsersCanRegister) {
                result = UtilsServices.GetResponse(ResponseType.None, T("Users cannot register due to site settings.").Text);
                return UtilsServices.ConvertToJsonResult(result);
            }
            try {
                _usersExtensionsServices.Register(userRegistrationParams);
                List<string> roles = new List<string>();
                var message = "";
                var registeredServicesData = UtilsServices.GetUserIdentityProviders(_identityProviders);
                var json = registeredServicesData.ToString();
                responseType = ResponseType.Success;
                if (OrchardServices.WorkContext.CurrentUser == null && registrationSettings.UsersMustValidateEmail) {
                    message = T("Thank you for registering. We sent you an e-mail with instructions to enable your account.").ToString();
                    responseType = ResponseType.ToConfirmEmail;
                }

                result = UtilsServices.GetResponse(responseType, message, json);
            }
            catch (Exception ex) {
                result = UtilsServices.GetResponse(ResponseType.None, ex.Message);
            }
            return UtilsServices.ConvertToJsonResult(result);
        }

        protected ContentResult SignInLogic(UserLogin login) {
            Response result;
            // If username or password are empty, send a MissingParameters response.
            if (string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password)) {
                result = UtilsServices.GetResponse(ResponseType.MissingParameters, T("Provide valid username and password").Text);
            } else {
                try {
                    var response = _usersExtensionsServices.SignIn(login);
                    if (response == ResponseType.Success) {
                        List<string> roles = new List<string>();
                        var registeredServicesData = UtilsServices.GetUserIdentityProviders(_identityProviders);
                        var json = registeredServicesData.ToString();
                        result = UtilsServices.GetResponse(ResponseType.Success, "", json);
                    } else {
                        result = UtilsServices.GetResponse(response, "");
                    }
                } catch (Exception ex) {
                    result = UtilsServices.GetResponse(ResponseType.InvalidUser, ex.Message);
                }
            }
            return UtilsServices.ConvertToJsonResult(result);
        }

        protected JsonResult SignOutLogic() {
            Response result;
            if (OrchardServices.WorkContext.CurrentUser == null // if the User is null the SignOutLogic do nothing and returns Success because the user is effectively not logged in
                || CsrfTokenHelper.DoesCsrfTokenMatchAuthToken()) {
                try {
                    _usersExtensionsServices.SignOut();
                    result = UtilsServices.GetResponse(ResponseType.Success);
                }
                catch (Exception ex) {
                    result = UtilsServices.GetResponse(ResponseType.InvalidUser, ex.Message);
                }
            }
            else {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(UtilsServices.GetResponse(ResponseType.InvalidXSRF));
            }
            return Json(result);
        }

        protected ContentResult GetCleanRegistrationPoliciesLogic(string lang = null) {
            var sb = new StringBuilder();
            var policies = _usersExtensionsServices.GetUserLinkedPolicies(lang);

            //policy.PendingPolicies
            sb.Insert(0, "{");
            sb.Append("\"Policies\":[");

            int i = 0;

            foreach (var item in policies) {
                if (i > 0) {
                    sb.Append(",");
                }
                sb.Append("{");
                sb.Append("\"PolicyId\":" + item.Id.ToString() + ",");
                sb.Append("\"Title\":\"" + item.ContentItem.As<TitlePart>().Title.Replace("\"", "\\\"") + "\",");
                sb.Append("\"Body\":\"" + item.ContentItem.As<BodyPart>().Text.Replace("\"", "\\\"").Replace("\r\n", "\\r\\n") + "\",");
                sb.Append("\"PolicyType\":\"" + item.PolicyType.ToString() + "\",");
                sb.Append("\"UserHaveToAccept\":" + item.UserHaveToAccept.ToString().ToLowerInvariant() + "");
                sb.Append("}");
                i++;
            }
            sb.Append("]");
            sb.Append("}");
            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }

        protected ContentResult GetRegistrationPoliciesLogic(string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, string lang = null, string complexBehaviour = "") {
            var sb = new StringBuilder();
            var _filterContentFieldsParts = mfilter.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            XElement projectionDump = null;
            // il dump dell'oggetto principale non filtra per field
            ObjectDumper dumper;
            var policies = _usersExtensionsServices.GetUserLinkedPolicies(lang);

            //policy.PendingPolicies
            sb.Insert(0, "{");
            sb.AppendFormat("\"n\": \"{0}\"", "Model");
            sb.AppendFormat(", \"v\": \"{0}\"", "VirtualContent");
            sb.Append(", \"m\": [{");
            sb.AppendFormat("\"n\": \"{0}\"", "VirtualId"); // Unused property for mobile mapper needs
            sb.AppendFormat(", \"v\": \"{0}\"", "0");
            sb.Append("}]");

            sb.Append(", \"l\":[");

            int i = 0;
            sb.Append("{");
            sb.AppendFormat("\"n\": \"{0}\"", "RegistrationPolicies");
            sb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
            sb.Append(", \"m\": [");

            foreach (var item in policies) {
                if (i > 0) {
                    sb.Append(",");
                }
                sb.Append("{");
                dumper = new ObjectDumper(deeplevel, _filterContentFieldsParts, false, tinyResponse, complexBehaviour.Split(','));
                projectionDump = dumper.Dump(item.ContentItem, String.Format("[{0}]", i));
                JsonConverter.ConvertToJSon(projectionDump, sb, minified, realformat);
                sb.Append("}");
                i++;
            }
            sb.Append("]");
            sb.Append("}");

            sb.Append("]"); // l : [
            sb.Append("}");
            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }

        protected JsonResult GetUserRegistrationModelLogic() {
            var userRegistration = new UserRegistration {
                Username = "MyUserName",
                Password = "MyPassword",
                ConfirmPassword = "MyPassword",
                PasswordQuestion = "MyPasswordQuestion",
                PasswordAnswer = "MyPasswordAnswer",
                Email = "myname@mydomain.it",
                Culture = "it-IT",
                PolicyAnswers = _usersExtensionsServices.GetUserLinkedPolicies("it-IT").Select(x => new UserPolicyAnswer {
                    PolicyId = x.Id,
                    UserHaveToAccept = x.UserHaveToAccept,
                    PolicyAnswer = false,
                    Policy = new PolicyTextViewModel {
                        Type = x.PolicyType,
                        Title = x.As<TitlePart>()?.Title,
                        Body = x.As<BodyPart>()?.Text
                    }
                }).ToList()
            };
            return Json(userRegistration, JsonRequestBehavior.AllowGet);
        }

        protected JsonResult RequestLostPasswordLogic(string username, LostPasswordUserOptions userOptions, string internationalPrefix = null) {
            // ensure users can request lost password
            Response result = UtilsServices.GetResponse(ResponseType.None, T("Send email failed.").Text);
            var registrationSettings = OrchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
            if (!registrationSettings.EnableLostPassword) {
                result = UtilsServices.GetResponse(ResponseType.None, T("Users cannot recover lost password due to site settings.").Text);

                return Json(result);
            }

            if (String.IsNullOrWhiteSpace(username)) {
                result = UtilsServices.GetResponse(ResponseType.None, T("Invalid user.").Text);
                return Json(result);
            }

            var siteUrl = OrchardServices.WorkContext.CurrentSite.BaseUrl;
            if (String.IsNullOrWhiteSpace(siteUrl)) {
                siteUrl = HttpContext.Request.ToRootUrlString();
            }

            // test if user is user/email or phone number
            if (userOptions == LostPasswordUserOptions.Account) {
                if (_userService.SendLostPasswordEmail(username, nonce => Url.MakeAbsolute(Url.Action("LostPassword", "Account", new { Area = "Orchard.Users", nonce = nonce }), siteUrl))) {
                    result = UtilsServices.GetResponse(ResponseType.Success);
                }
                else {
                    result = UtilsServices.GetResponse(ResponseType.None, T("Send email failed.").Text);
                }
            }
            else {
                var sendSmsResult = _usersExtensionsServices.SendLostPasswordSms(internationalPrefix, username, nonce => Url.MakeAbsolute(Url.Action("LostPassword", "Account", new { Area = "Orchard.Users", nonce = nonce }), siteUrl));

                if (sendSmsResult == "TRUE") {
                    result = UtilsServices.GetResponse(ResponseType.Success);
                }
                else {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    errors.Add("BODYEXCEEDED", T("Message rejected: too many characters. (160 max)").ToString()); //"messaggio rigettato per superamento lunghezza max di testo (160 caratteri)");
                    errors.Add("MISSINGPARAMETER_1", T("Missing recipient").ToString()); //"Destinatario mancante");
                    errors.Add("MISSINGPARAMETER_2", T("Sender identifier missing").ToString()); //"Identificativo di invio mancante");
                    errors.Add("MISSINGPARAMETER_3", T("Sender missing or wrong").ToString()); //"Mittente mancante o errato");
                    errors.Add("MISSINGPARAMETER_4", T("Missing text").ToString()); //"Testo mancante");
                    errors.Add("MISSINGPARAMETER_5", T("Priority missing or wrong").ToString()); //"Priorità mancante o errata");
                    errors.Add("FALSE", T("Generic error").ToString()); //"Errore generico");
                    result = UtilsServices.GetResponse(ResponseType.None, T("Send SMS failed.").Text + errors[sendSmsResult].ToString());
                }
            }
            return Json(result);
        }

        protected JsonResult ChallengeEmailApiLogic(string nonce) {
            var user = _userService.ValidateChallenge(nonce);
            Response result;
            if (user != null) {
                _userEventHandler.ConfirmedEmail(user);

                result = UtilsServices.GetResponse(ResponseType.Success, T("Email confirmed").Text);

                return Json(result);
            }
            result = UtilsServices.GetResponse(ResponseType.None, T("Email not confirmed").Text);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        protected ContentResult ChangePasswordLogic(string currentPassword, string newPassword, string confirmPassword) {
            return ChangePasswordLogicInternal(currentPassword, newPassword, confirmPassword, OrchardServices.WorkContext.CurrentUser.UserName);
        }

        protected ContentResult ChangeExpiredPasswordLogic(string currentPassword, string newPassword, string confirmPassword, string userName) {
            return ChangePasswordLogicInternal(currentPassword, newPassword, confirmPassword, userName);
        }

        protected ContentResult ChangePasswordLogicInternal(string currentPassword, string newPassword, string confirmPassword, string userName) {
            Response result;

            var membershipSettings = _membershipService.GetSettings();

            // Using string.IsNullOrEmpty because AccountController does that
            if (string.IsNullOrEmpty(currentPassword) ||
                    string.IsNullOrEmpty(newPassword) ||
                    string.IsNullOrEmpty(confirmPassword)) {
                result = UtilsServices.GetResponse(ResponseType.MissingParameters, T("You must specify current password, new password and confirm password.").Text);
                return UtilsServices.ConvertToJsonResult(result);
            }

            if (string.Equals(currentPassword, newPassword, StringComparison.Ordinal)) {
                result = UtilsServices.GetResponse(ResponseType.Validation, T("The new password must be different from the current password.").Text);
                return UtilsServices.ConvertToJsonResult(result);
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal)) {
                result = UtilsServices.GetResponse(ResponseType.Validation, T("New password and confirm password must be equal.").Text);
                return UtilsServices.ConvertToJsonResult(result);
            }

            // Try to validate user + password combination.
            // During this call, user should be authenticated, so CurrentUser can be used.
            var user = _membershipService.ValidateUser(userName, currentPassword);
            if (user == null) {
                result = UtilsServices.GetResponse(ResponseType.Validation, T("The current password provided is not valid.").Text);
                return UtilsServices.ConvertToJsonResult(result);
            }

            // Check if new password meets password policies.
            IDictionary<string, LocalizedString> validationErrors;
            _userService.PasswordMeetsPolicies(newPassword, user, out validationErrors);
            if (validationErrors != null && validationErrors.Any()) {
                var errorString = T("New password does not meet password policies: ").Text;
                foreach (var error in validationErrors) {
                    errorString += Environment.NewLine + error.Value.Text;
                }

                result = UtilsServices.GetResponse(ResponseType.Validation, errorString);
                return UtilsServices.ConvertToJsonResult(result);
            }

            var shouldSignout = OrchardServices.WorkContext
                .CurrentSite.As<SecuritySettingsPart>()
                .ShouldInvalidateAuthOnPasswordChanged;

            _userEventHandler.ChangingPassword(user, newPassword);
            _membershipService.SetPassword(user, newPassword);
            _userEventHandler.ChangedPassword(user);
            if (shouldSignout) {
                _usersExtensionsServices.SignOut();
            }

            result = UtilsServices.GetResponse(ResponseType.Success, T("Password changed.").Text);
            return UtilsServices.ConvertToJsonResult(result);
        }

        protected ContentResult ChangeLostPasswordLogic(string nonce, string newPassword, string confirmPassword) {
            Response result;

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal)) {
                result = UtilsServices.GetResponse(ResponseType.Validation, T("New password and confirm password must be equal.").Text);
                return UtilsServices.ConvertToJsonResult(result);
            }

            IUser user = _userService.ValidateLostPassword(nonce);

            if (user == null) {
                result = UtilsServices.GetResponse(ResponseType.InvalidUser, "Invalid user.");
                return UtilsServices.ConvertToJsonResult(result);
            }

            // Check if new password meets password policies.
            IDictionary<string, LocalizedString> validationErrors;
            _userService.PasswordMeetsPolicies(newPassword, user, out validationErrors);
            if (validationErrors != null && validationErrors.Any()) {
                var errorString = T("New password does not meet password policies: ").Text;
                foreach (var error in validationErrors) {
                    errorString += Environment.NewLine + error.Value.Text;
                }

                result = UtilsServices.GetResponse(ResponseType.Validation, errorString);
                return UtilsServices.ConvertToJsonResult(result);
            }

            _userEventHandler.ChangingPassword(user, newPassword);
            _membershipService.SetPassword(user, newPassword);
            _userEventHandler.ChangedPassword(user);

            result = UtilsServices.GetResponse(ResponseType.Success, T("Password changed.").Text);
            return UtilsServices.ConvertToJsonResult(result);
        }

        protected ContentResult SendChallengeEmailLogic(string username) {
            Response result;

            // Check users must confirm their account on registration.
            var membershipSettings = _membershipService.GetSettings();
            if (!membershipSettings.UsersMustValidateEmail) {
                result = UtilsServices.GetResponse(ResponseType.None, T("Invalid operation: users do not need to validate their email.").Text);
                return UtilsServices.ConvertToJsonResult(result);
            }

            if (string.IsNullOrWhiteSpace(username)) {
                result = UtilsServices.GetResponse(ResponseType.MissingParameters, T("You must specify a username or e-mail.").Text);
                return UtilsServices.ConvertToJsonResult(result);
            }

            var user = _userService.GetUserByNameOrEmail(username);
            if (user != null && user.EmailStatus == UserStatus.Pending) {
                var siteUrl = OrchardServices.WorkContext.CurrentSite.BaseUrl;
                _userService.SendChallengeEmail(user.As<UserPart>(), nonce => Url.MakeAbsolute(Url.Action("ChallengeEmail", "Account", new { Area = "Orchard.Users", nonce = nonce }), siteUrl));
                _userEventHandler.SentChallengeEmail(user);
                
                result = UtilsServices.GetResponse(ResponseType.Success, T("Challenge email sent.").Text);
                return UtilsServices.ConvertToJsonResult(result);
            } else {
                result = UtilsServices.GetResponse(ResponseType.InvalidUser, T("Invalid username or e-mail.").Text);
                return UtilsServices.ConvertToJsonResult(result);
            }
        }
    }
}