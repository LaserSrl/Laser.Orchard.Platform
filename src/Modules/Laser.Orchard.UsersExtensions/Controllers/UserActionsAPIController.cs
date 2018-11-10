using System.Web.Http;
using Laser.Orchard.Mobile.ViewModels;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.UsersExtensions.Models;
using Laser.Orchard.UsersExtensions.Services;

namespace Laser.Orchard.UsersExtensions.Controllers {


    public class UserActionsAPIController : ApiController {
  
            //private readonly ICsrfTokenHelper _csrfTokenHelper;
            //private readonly IUsersExtensionsServices _usersExtensionsServices;
            //private readonly IControllerContextAccessor _controllerContextAccessor;
            //private readonly IOrchardServices _orchardServices;
            //private readonly IUserService _userService;
            //private readonly IUtilsServices _utilsServices;
            private readonly IUserActionMethods _userActionMethods;
          //  public ILogger Log { get; set; }

            public UserActionsAPIController(
                //IOrchardServices orchardServices, ICsrfTokenHelper csrfTokenHelper, IUsersExtensionsServices usersExtensionsServices, IUserService userService,IControllerContextAccessor controllerContextAccessor, IUtilsServices utilsServices,
                IUserActionMethods userActionMethods) {
                //_csrfTokenHelper = csrfTokenHelper;
                //_usersExtensionsServices = usersExtensionsServices;
                //_controllerContextAccessor = controllerContextAccessor;
                //_orchardServices = orchardServices;
                //_userService = userService;
                //T = NullLocalizer.Instance;
                //Log = NullLogger.Instance;
                //_utilsServices = utilsServices;
                _userActionMethods = userActionMethods;
            }

         //   public Localizer T { get; set; }

            #region [https calls]

            /// <summary>
            /// </summary>
            /// <param name="userRegistrationParams">
            /// </param>
            /// <returns></returns>
            [System.Web.Mvc.HttpPost]
            public Response RegisterSsl(UserRegistration userRegistrationParams) {
                return _userActionMethods.RegisterLogic(userRegistrationParams);
            }

            [System.Web.Mvc.HttpPost]
            public Response SignInSsl(UserLogin login) {
                return _userActionMethods.SignInLogic(login);
            }

            [System.Web.Mvc.HttpPost]
            public Response SignOutSsl() {
                return _userActionMethods.SignOutLogic();
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
            [System.Web.Mvc.HttpPost]
            public Response RequestLostPasswordSmsSsl(PhoneNumberViewModel phoneNumber) {
                return _userActionMethods.RequestLostPasswordLogic(phoneNumber.PhoneNumber, LostPasswordUserOptions.Phone, phoneNumber.InternationalPrefix);
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
            [System.Web.Mvc.HttpPost]
            public Response RequestLostPasswordAccountOrEmailSsl(string username) {
                return _userActionMethods.RequestLostPasswordLogic(username, LostPasswordUserOptions.Account);
            }

  
            public string GetCleanRegistrationPoliciesSsl(string lang = null) {
                return _userActionMethods.GetCleanRegistrationPoliciesLogic(lang);
            }


            public string GetRegistrationPoliciesSsl(string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, string lang = null) {
                return _userActionMethods.GetRegistrationPoliciesLogic(mfilter, page, pageSize, tinyResponse, minified, realformat, deeplevel, lang);
            }

   

            public UserRegistration GetUserRegistrationModelSsl() {
                return _userActionMethods.GetUserRegistrationModelLogic();
            }

            #endregion [https calls]

            #region [http calls]

            [System.Web.Mvc.HttpPost]
            public Response Register(UserRegistration userRegistrationParams) {
                return _userActionMethods.RegisterLogic(userRegistrationParams);
            }

            [System.Web.Mvc.HttpPost]
            public Response SignIn(UserLogin login) {
                return _userActionMethods.SignInLogic(login);
            }

            [System.Web.Mvc.HttpPost]
            public Response SignOut() {
                return _userActionMethods.SignOutLogic();
            }

            [System.Web.Mvc.HttpPost]
            public Response RequestLostPasswordSms(PhoneNumberViewModel phoneNumber) {
                return _userActionMethods.RequestLostPasswordLogic(phoneNumber.PhoneNumber, LostPasswordUserOptions.Phone, phoneNumber.InternationalPrefix);
            }

            [System.Web.Mvc.HttpPost]
            public Response RequestLostPasswordAccountOrEmail(string username) {
                return _userActionMethods.RequestLostPasswordLogic(username, LostPasswordUserOptions.Account);
            }


            //public string GetCleanRegistrationPolicies(string lang = null) {
            //    return Json.Parser(_userActionMethods.GetCleanRegistrationPoliciesLogic(lang));
            //}


            //public string GetRegistrationPolicies(string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, string lang = null) {
            //    return _userActionMethods.GetRegistrationPoliciesLogic(mfilter, page, pageSize, tinyResponse, minified, realformat, deeplevel, lang);
            //}


            public UserRegistration GetUserRegistrationModel() {
                return _userActionMethods.GetUserRegistrationModelLogic();
            }

            #endregion [http calls]

        }


}