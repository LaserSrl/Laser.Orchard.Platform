using System;
using System.Web;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Security;
using Orchard.Users.Events;

namespace Laser.Orchard.Policy.Handlers {
    public class PolicyUserEventHandler : IUserEventHandler {
        private readonly IPolicyServices _policyService;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly IOrchardServices _orchardServices;

        public PolicyUserEventHandler(IPolicyServices policyService, IControllerContextAccessor controllerContextAccessor, IOrchardServices orchardServices) {
            _policyService = policyService;
            _controllerContextAccessor = controllerContextAccessor;
            _orchardServices = orchardServices;
        }

        public void LoggedIn(IUser user) {
            try {
                var policy = _policyService.GetPoliciesForCurrentUser(true); // attacca il cookie delle policy quando un utente si logga
            } catch { }
        }

        public void LoggedOut(IUser user) {
            HttpCookie currentPoliciesAnswers = _orchardServices.WorkContext.HttpContext.Request.Cookies["PoliciesAnswers"];
            if (currentPoliciesAnswers != null) {
                // may be the cookie is already not set
                _orchardServices.WorkContext.HttpContext.Response.Cookies.Remove("PoliciesAnswers");
                currentPoliciesAnswers.Expires = DateTime.Now.AddDays(-10);
                currentPoliciesAnswers.Value = null;
                _orchardServices.WorkContext.HttpContext.Response.SetCookie(currentPoliciesAnswers);
            }
        }

        #region Unused events
        public void Approved(IUser user) {
        }

        public void Moderate(IUser user) {
        }

        public void Created(UserContext context) {
        }

        public void Creating(UserContext context) {
        }
        
        public void AccessDenied(IUser user) {
        }

        public void ChangedPassword(IUser user) {
        }

        public void SentChallengeEmail(IUser user) {
        }

        public void ConfirmedEmail(IUser user) {
        }

        public void LoggingIn(string userNameOrEmail, string password) {
        }

        public void LogInFailed(string userNameOrEmail, string password) {
        }

        #endregion
    }
}