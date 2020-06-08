using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.Policy.ViewModels;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.UsersExtensions.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Localization.Records;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Utility.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.UsersExtensions.Services {
    public interface IUsersExtensionsServices : IDependency {
        void Register(UserRegistration userRegistrationParams);
        void SignIn(UserLogin userLoginParams);
        void SignOut();
        IEnumerable<PolicyTextInfoPart> GetUserLinkedPolicies(string culture = null);
        bool ValidateRegistration(string userName, string email, string password, string confirmPassword, out List<string> errors);
        IList<UserPolicyAnswerWithContent> BuildEditorForRegistrationPolicies();
        string SendLostPasswordSms(string internationalPrefix, string phoneNumber, Func<string, string> createUrl);
        UserPart GetUserByMail(string mail);
        IList<UserPolicyAnswerWithContent> BuildEditorForPolicies(PolicyPart policyPart);
    }


    public class UsersExtensionsServices : IUsersExtensionsServices {
        private readonly IOrchardServices _orchardServices;
        private readonly IPolicyServices _policyServices;
        private readonly IMembershipService _membershipService;
        private readonly IUtilsServices _utilsServices;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserService _userService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IShapeFactory _shapeFactory;
        private ISmsServices _smsServices;
        private readonly ICultureManager _cultureManager;
        private readonly IAccountValidationService _accountValidationService;

        private static readonly TimeSpan DelayToResetPassword = new TimeSpan(1, 0, 0, 0); // 24 hours to reset password
        private readonly ICommonsServices _commonsServices;


        public UsersExtensionsServices(
            IOrchardServices orchardServices,
            IPolicyServices policySerivces,
            IMembershipService membershipService,
            IUtilsServices utilsServices,
            IAuthenticationService authenticationService,
            IUserService userService,
            IUserEventHandler userEventHandler,
            IShapeFactory shapeFactory,
            ICultureManager cultureManager,
            ICommonsServices commonsServices,
            IAccountValidationService accountValidationService) {

            T = NullLocalizer.Instance;
            Log = NullLogger.Instance;
            _policyServices = policySerivces;
            _orchardServices = orchardServices;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _utilsServices = utilsServices;
            _userService = userService;
            _userEventHandler = userEventHandler;
            _shapeFactory = shapeFactory;
            _cultureManager = cultureManager;
            _commonsServices = commonsServices;
            _accountValidationService = accountValidationService;
        }

        public Localizer T { get; set; }

        private ILogger Log { get; set; }

        int MinPasswordLength {
            get {
                return _membershipService.GetSettings().GetMinimumPasswordLength();
            }
        }

        public void Register(UserRegistration userRegistrationParams) {
            if (RegistrationSettings.UsersCanRegister) {
                var policyAnswers = new List<PolicyForUserViewModel>();
                if (_utilsServices.FeatureIsEnabled("Laser.Orchard.Policy")
                    && UserRegistrationExtensionsSettings.IncludePendingPolicy == Policy.IncludePendingPolicyOptions.Yes) {
                    IEnumerable<PolicyTextInfoPart> policies = GetUserLinkedPolicies(userRegistrationParams.Culture);
                    // controllo che tutte le policy abbiano una risposta e che le policy obbligatorie siano accettate 
                    var allRight = true;
                    foreach (var policy in policies) {
                        var policyId = policy.Id;
                        var policyRequired = policy.UserHaveToAccept;
                        var answer = userRegistrationParams.PolicyAnswers.Where(w => w.PolicyId == policyId).SingleOrDefault();
                        if (answer != null) {
                            if (!answer.PolicyAnswer && policyRequired) {
                                allRight = false;
                            }
                        }
                        else if (answer == null && policyRequired) {
                            allRight = false;
                        }
                        if (answer != null) {
                            policyAnswers.Add(new PolicyForUserViewModel {
                                OldAccepted = false,
                                PolicyTextId = policyId,
                                Accepted = answer.PolicyAnswer,
                                AnswerDate = DateTime.Now
                            });
                        }
                    }
                    if (!allRight) {
                        throw new SecurityException(T("User has to accept policies!").Text);
                    }
                }
                var registrationErrors = new List<string>();
                if (ValidateRegistration(userRegistrationParams.Username, userRegistrationParams.Email,
                    userRegistrationParams.Password, userRegistrationParams.ConfirmPassword, out registrationErrors)) {

                    var createdUser = _membershipService.CreateUser(new CreateUserParams(
                        userRegistrationParams.Username,
                        userRegistrationParams.Password,
                        userRegistrationParams.Email,
                        userRegistrationParams.PasswordQuestion,
                        userRegistrationParams.PasswordAnswer,
                        (RegistrationSettings.UsersAreModerated == false) && (RegistrationSettings.UsersMustValidateEmail == false)
                        ));
                    // _membershipService.CreateUser may return null and tell nothing about why it failed to create the user
                    // if the Creating user event handlers set the flag to cancel user creation.
                    if (createdUser == null) {
                        throw new SecurityException(T("User registration failed.").Text);
                    }
                    // here user was created
                    var favCulture = createdUser.As<FavoriteCulturePart>();
                    if (favCulture != null) {
                        var culture = _commonsServices.ListCultures().SingleOrDefault(x => x.Culture.Equals(userRegistrationParams.Culture));
                        if (culture != null) {
                            favCulture.Culture_Id = culture.Id;
                        }
                        else {
                            // usa la culture di default del sito
                            favCulture.Culture_Id = _cultureManager.GetCultureByName(_cultureManager.GetSiteCulture()).Id;
                        }
                    }
                    if ((RegistrationSettings.UsersAreModerated == false) && (RegistrationSettings.UsersMustValidateEmail == false)) {
                        _userEventHandler.LoggingIn(userRegistrationParams.Username, userRegistrationParams.Password);
                        _authenticationService.SignIn(createdUser, userRegistrationParams.CreatePersistentCookie);
                        // solleva l'evento LoggedIn sull'utente
                        _userEventHandler.LoggedIn(createdUser);
                    }

                    // [HS] BEGIN: Whe have to save the PoliciesAnswers cookie and persist answers on the DB after Login/SignIn events because during Login/Signin events database is not updated yet and those events override cookie in an unconsistent way.
                    if (_utilsServices.FeatureIsEnabled("Laser.Orchard.Policy") && UserRegistrationExtensionsSettings.IncludePendingPolicy == Policy.IncludePendingPolicyOptions.Yes) {
                        _policyServices.PolicyForUserMassiveUpdate(policyAnswers, createdUser);
                    }
                    // [HS] END

                    if (RegistrationSettings.UsersMustValidateEmail) {
                        // send challenge e-mail
                        var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
                        if (string.IsNullOrWhiteSpace(siteUrl)) {
                            siteUrl = HttpContext.Current.Request.ToRootUrlString();
                        }
                        UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                        _userService.SendChallengeEmail(createdUser, nonce => urlHelper.MakeAbsolute(urlHelper.Action("ChallengeEmail", "Account", new { Area = "Orchard.Users", nonce = nonce }), siteUrl));
                    }
                }
                else {
                    throw new SecurityException(String.Join(", ", registrationErrors));
                }
            }
            else {
                throw new SecurityException(T("User cannot register due to Site settings").Text);
            }
        }

        public void SignIn(UserLogin userLoginParams) {
            var user = _membershipService.ValidateUser(userLoginParams.Username, userLoginParams.Password);
            if (user != null) {
                _userEventHandler.LoggingIn(userLoginParams.Username, userLoginParams.Password);
                _authenticationService.SignIn(user, userLoginParams.CreatePersistentCookie);
                _userEventHandler.LoggedIn(user);
            }
            else {
                throw new SecurityException(T("The username or e-mail or password provided is incorrect.").Text);
            }
        }

        public void SignOut() {
            _authenticationService.SignOut();

            var loggedUser = _authenticationService.GetAuthenticatedUser();
            if (loggedUser != null) {
                _userEventHandler.LoggedOut(loggedUser);
            }
        }

        public string SendLostPasswordSms(string internationalPrefix, string phoneNumber, Func<string, string> createUrl) {
            _orchardServices.WorkContext.TryResolve<ISmsServices>(out _smsServices);
            if (_smsServices == null) return "FALSE";

            var user = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>()
                .Join<UserPwdRecoveryPartRecord>()
                .Where(u => u.InternationalPrefix == internationalPrefix.ToString() && u.PhoneNumber == phoneNumber.ToString())
                .List().FirstOrDefault();

            if (user != null) {
                string nonce = _userService.CreateNonce(user, DelayToResetPassword);
                string url = createUrl(nonce);

                //var template = _shapeFactory.Create("Template_User_LostPassword", Arguments.From(new {
                //    User = user,
                //    LostPasswordUrl = url
                //}));
                //template.Metadata.Wrappers.Add("Template_User_Wrapper");

                //var parameters = new Dictionary<string, object> {
                //            {"Subject", T("Lost password").Text},
                //            {"Body", _shapeDisplay.Display(template)},
                //            {"Recipients", user.Email }
                //        };
                var smsSettings = _orchardServices.WorkContext.CurrentSite.As<SmsSettingsPart>();

                long phoneNumberComplete = 0;
                if (long.TryParse(String.Concat(internationalPrefix.Trim(), phoneNumber.Trim()), out phoneNumberComplete)) {
                    //return _smsServices.SendSms(new long[] { phoneNumberComplete }, user.UserName + "\r\n" + url);

                    Hashtable hs = new Hashtable();
                    hs.Add("SmsContactNumber", phoneNumberComplete);

                    List<Hashtable> listaDestinatari = new List<Hashtable>();
                    listaDestinatari.Add(hs);

                    return _smsServices.SendSms(listaDestinatari, user.UserName + "\r\n" + url);
                }
            }

            return "FALSE";
        }

        public IEnumerable<PolicyTextInfoPart> GetUserLinkedPolicies(string culture = null) {
            IEnumerable<PolicyTextInfoPart> policies;
            if (UserRegistrationExtensionsSettings.IncludePendingPolicy == Policy.IncludePendingPolicyOptions.No)
                return new List<PolicyTextInfoPart>(); // se selezionato No allora nessuna policy è obbligatoria e ritorno una collection vuota
            if (UserRegistrationExtensionsSettings.PolicyTextReferences.FirstOrDefault() == null
                || UserRegistrationExtensionsSettings.PolicyTextReferences.FirstOrDefault() == "{All}") {
                policies = _policyServices.GetPolicies(culture);
            }
            else {
                var ids = UserRegistrationExtensionsSettings
                    .PolicyTextReferences.Select(x => Convert.ToInt32(x.Replace("{", "").Replace("}", ""))).ToArray();
                policies = _policyServices.GetPolicies(culture, ids);
            }
            return policies;
        }

        public bool ValidateRegistration(string userName, string email, string password, string confirmPassword, out List<string> errors) {

            errors = new List<string>();

            IDictionary<string, LocalizedString> validationErrors;

            var validate = _accountValidationService.ValidateUserName(userName, out validationErrors);
            if (!validate) {
                foreach (var error in validationErrors) {
                    errors.Add(string.Format("{0}: {1}", error.Key, error.Value.Text));
                }
            }

            validate &= _accountValidationService.ValidateEmail(email, out validationErrors);
            if (!validate) {
                foreach (var error in validationErrors) {
                    errors.Add(string.Format("{0}: {1}", error.Key, error.Value.Text));
                }
            }

            if (!validate)
                return false;

            if (!_userService.VerifyUserUnicity(userName, email)) {
                errors.Add(T("User with that username and/or email already exists.").Text);
            }

            if (!_accountValidationService.ValidatePassword(password, out validationErrors)) {
                foreach (var error in validationErrors) {
                    errors.Add(string.Format("{0}: {1}", error.Key, error.Value.Text));
                }
            }

            if (!String.Equals(password, confirmPassword, StringComparison.Ordinal)) {
                errors.Add(T("The new password and confirmation password do not match.").Text);
            }
            return errors.Count == 0;
        }

        public IList<UserPolicyAnswerWithContent> BuildEditorForRegistrationPolicies() {
            var policies = GetUserLinkedPolicies().Select(x => new UserPolicyAnswerWithContent {
                PolicyAnswer = false,
                PolicyId = x.Id,
                UserHaveToAccept = x.UserHaveToAccept,
                PolicyText = x.ContentItem,
                Policy = new PolicyTextViewModel {
                    Type = x.PolicyType,
                    Title = x.As<TitlePart>()?.Title,
                    Body = x.As<BodyPart>()?.Text
                }
            }).ToList();
            return policies;
        }
        public IList<UserPolicyAnswerWithContent> BuildEditorForPolicies(PolicyPart policyPart) {
            var associatedPolicies = _policyServices.GetPoliciesForContent(policyPart); //Reading policies Ids for that content. Ids are in strings i.e. "{12}"
            if (associatedPolicies.Count() == 0) return new List<UserPolicyAnswerWithContent>();

            var contentPolicies = _policyServices.GetPolicies(null, associatedPolicies.Select(x => Convert.ToInt32(x.Substring(1,x.Length-2)/*Strips {} chars*/)).ToArray());
            return contentPolicies.Select(x => new UserPolicyAnswerWithContent {
                PolicyAnswer = false,
                PolicyId = x.Id,
                UserHaveToAccept = x.UserHaveToAccept,
                PolicyText = x.ContentItem,
                Policy = new PolicyTextViewModel {
                    Type = x.PolicyType,
                    Title = x.As<TitlePart>()?.Title,
                    Body = x.As<BodyPart>()?.Text
                }
            }).ToList();

        }
        public UserPart GetUserByMail(string mail) {
            var qry = _orchardServices.ContentManager.Query("User").Where<UserPartRecord>(x => x.Email == mail);
            var usr = qry.Slice(0, 1).FirstOrDefault();
            if (usr != null) {
                return usr.As<UserPart>();
            }
            return null;
        }

        private RegistrationSettingsPart RegistrationSettings {
            get {
                var orchardUsersSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
                return orchardUsersSettings;
            }
        }

        private UserRegistrationSettingsPart UserRegistrationExtensionsSettings {
            get {

                var orchardUsersSettings = _orchardServices.WorkContext.CurrentSite.As<UserRegistrationSettingsPart>();
                return orchardUsersSettings;
            }
        }

    }
}