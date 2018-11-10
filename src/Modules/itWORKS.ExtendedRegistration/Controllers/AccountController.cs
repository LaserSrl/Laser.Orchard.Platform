using Contrib.Profile.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Users.Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Security;

namespace itWORKS.ExtendedRegistration.Controllers {
    [ValidateInput(false), Themed]
    public class AccountController : Controller, IUpdateModel {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;
        private readonly IOrchardServices _orchardServices;
        //private readonly IShapeFactory shapeFactory;
        private readonly IContentManager _contentManager;
        private readonly IFrontEndProfileService _frontEndProfileService;
        private readonly ShellSettings _shellSettings;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IAccountValidationService _accountValidationService;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        dynamic Shape { get; set; }

        int MinPasswordLength {
            get {
                return _membershipService.GetSettings().GetMinimumPasswordLength();
            }
        }

        public AccountController(
          IAuthenticationService authenticationService,
          IMembershipService membershipService,
          IUserService userService,
          IOrchardServices orchardServices,
          IShapeFactory shapeFactory,
          IContentManager contentManager,
          IFrontEndProfileService frontEndProfileService,
          ShellSettings shellSetting,
            IUserEventHandler userEventHandler,
            IAccountValidationService accountValidationService) {

            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _userService = userService;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            _contentManager = contentManager;
            _frontEndProfileService = frontEndProfileService;
            _shellSettings = shellSetting;
            _userEventHandler = userEventHandler;
            _accountValidationService = accountValidationService;
        }

        public ActionResult Register() {
            // ensure users can register
            var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
            if (!registrationSettings.UsersCanRegister) {
                return HttpNotFound();
            }

            ViewData["PasswordLength"] = MinPasswordLength;

            var shape = _orchardServices.New.Register();

            var user = _orchardServices.ContentManager.New("User");
            if (user != null && !_frontEndProfileService.UserHasNoProfilePart(user.As<IUser>())) {
                shape.UserProfile = _frontEndProfileService.BuildFrontEndShape(
                    _contentManager.BuildEditor(user),
                    _frontEndProfileService.MayAllowPartEdit,
                    _frontEndProfileService.MayAllowFieldEdit);
            }

            return new ShapeResult(this, shape);
        }

        [HttpPost]
        public ActionResult Register(string userName, string email, string password, string confirmPassword, string returnUrl = null,bool createPersistentCookie=false) {
            if (string.IsNullOrEmpty(returnUrl)) {
                returnUrl = Request.QueryString["ReturnUrl"];
            }
            // ensure users can register
            var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
            if (!registrationSettings.UsersCanRegister) {
                return HttpNotFound();
            }
            ViewData["PasswordLength"] = MinPasswordLength;

            var shape = _orchardServices.New.Register();

            // validate user part, create a temp user part to validate input
            var userPart = _contentManager.New("User");
            if (userPart != null && !_frontEndProfileService.UserHasNoProfilePart(userPart.As<IUser>())) {
                shape.UserProfile = _frontEndProfileService.BuildFrontEndShape(
                    _contentManager.UpdateEditor(userPart, this),
                    _frontEndProfileService.MayAllowPartEdit,
                    _frontEndProfileService.MayAllowFieldEdit);
                if (!ModelState.IsValid) {
                    _orchardServices.TransactionManager.Cancel();
                    return new ShapeResult(this, shape);
                }
            }

            if (ValidateRegistration(userName, email, password, confirmPassword)) {
                // Attempt to register the user
                // No need to report this to IUserEventHandler because _membershipService does that for us
                var user = _membershipService.CreateUser(new CreateUserParams(userName, password, email, null, null, false));

                if (user != null) {
                    if (!_frontEndProfileService.UserHasNoProfilePart(user)) {
                        // we know userpart data is ok, now we update the 'real' recently published userpart
                        _frontEndProfileService.BuildFrontEndShape(
                            _contentManager.UpdateEditor(user, this),
                            _frontEndProfileService.MayAllowPartEdit,
                            _frontEndProfileService.MayAllowFieldEdit);
                    }

                    var userPart2 = user.As<UserPart>();
                    if (user.As<UserPart>().EmailStatus == UserStatus.Pending) {
                        _userService.SendChallengeEmail(user.As<UserPart>(), nonce => Url.AbsoluteAction(() => Url.Action("ChallengeEmail", "Account", new { Area = "Orchard.Users", nonce = nonce })));
                        
                        _userEventHandler.SentChallengeEmail(user);
                        return RedirectToAction("ChallengeEmailSent", "Account", new { area = "Orchard.Users" });
                    }

                    if (user.As<UserPart>().RegistrationStatus == UserStatus.Pending) {
                        return RedirectToAction("RegistrationPending", "Account", new { area = "Orchard.Users" });
                    }

                    _userEventHandler.LoggingIn(userName, password);
                    _authenticationService.SignIn(user, createPersistentCookie );
                    _userEventHandler.LoggedIn(user);

                    if (!string.IsNullOrEmpty(returnUrl)) {
                        return this.RedirectLocal(returnUrl);
                    }

                    return Redirect(string.IsNullOrWhiteSpace(_shellSettings.RequestUrlPrefix) ?
                        "~/" :
                        "~/" + _shellSettings.RequestUrlPrefix.Trim('/'));
                }

                ModelState.AddModelError("_FORM", T(ErrorCodeToString(/*createStatus*/MembershipCreateStatus.ProviderError)));
            }

            // If we got this far, something failed, redisplay form
            //var shape = _orchardServices.New.Register();
            return new ShapeResult(this, shape);
        }

        private bool ValidateRegistration(string userName, string email, string password, string confirmPassword) {

            IDictionary<string, LocalizedString> validationErrors;

            var validate = _accountValidationService.ValidateUserName(userName, out validationErrors);
            if (!validate) {
                foreach (var error in validationErrors) {
                    ModelState.AddModelError(error.Key, error.Value);
                }
            }

            validate &= _accountValidationService.ValidateEmail(email, out validationErrors);
            if (!validate) {
                foreach (var error in validationErrors) {
                    ModelState.AddModelError(error.Key, error.Value);
                }
            }

            if (!validate)
                return false;

            if (!_userService.VerifyUserUnicity(userName, email)) {
                ModelState.AddModelError("userExists", T("User with that username and/or email already exists."));
            }

            ValidatePassword(password);

            if (!String.Equals(password, confirmPassword, StringComparison.Ordinal)) {
                ModelState.AddModelError("_FORM", T("The new password and confirmation password do not match."));
            }
            return ModelState.IsValid;
        }

        private void ValidatePassword(string password) {
            IDictionary<string, LocalizedString> validationErrors;

            if (!_accountValidationService.ValidatePassword(password, out validationErrors)) {
                foreach (var error in validationErrors) {
                    ModelState.AddModelError(error.Key, error.Value);
                }
            }

        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus) {
            // See http://msdn.microsoft.com/en-us/library/system.web.security.membershipcreatestatus.aspx for
            // a full list of status codes.
            switch (createStatus) {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return
                        "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return
                        "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return
                        "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }

}