using System;
using Laser.Orchard.OpenAuthentication.Services;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.ContentManagement;
using Orchard.UI.Notify;
using Orchard.Localization;
using Orchard.Users.Services;

namespace Laser.Orchard.OpenAuthentication.Events {
    public class OpenAuthUserEventHandler : IUserEventHandler {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrchardOpenAuthWebSecurity _orchardOpenAuthWebSecurity;
        private readonly INotifier _notifier;
        private readonly IAuthenticationService _authenticationService;
        private readonly IContentManager _contentManager;


        public OpenAuthUserEventHandler(IHttpContextAccessor httpContextAccessor,
            IOrchardOpenAuthWebSecurity orchardOpenAuthWebSecurity,
            IContentManager contentManager,
            IAuthenticationService authenticationService,
            INotifier notifier) {
            _httpContextAccessor = httpContextAccessor;
            _orchardOpenAuthWebSecurity = orchardOpenAuthWebSecurity;
            _authenticationService = authenticationService;
            _notifier = notifier;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Creating(UserContext context) {
        }

        public void Created(UserContext context) {
            CreateOrUpdateOpenAuthUser(context.User);
        }

        public void LoggedIn(IUser user) {
            CreateOrUpdateOpenAuthUser(user);
        }

        private void CreateOrUpdateOpenAuthUser(IUser user) {
            var current = _httpContextAccessor.Current();
            if (current == null)
                return;

            var request = current.Request;

            if (request == null)
                return;

            var userName = request.QueryString["UserName"];
            var externalLoginData = request.QueryString["ExternalLoginData"];

            if (string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(externalLoginData))
                return;

            string providerName;
            string providerUserId;

            if (
                !_orchardOpenAuthWebSecurity.TryDeserializeProviderUserId(externalLoginData, out providerName,
                                                                          out providerUserId))
                return;

            _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(providerName, providerUserId, user);
        }

        public void LoggedOut(IUser user) {
        }

        public void AccessDenied(IUser user) {
        }

        public void ChangedPassword(IUser user) {
        }

        public void SentChallengeEmail(IUser user) {
        }

        public void ConfirmedEmail(IUser user) {
            MergeUserToClosestMergeable(user);
        }

        public void Approved(IUser user) {
            MergeUserToClosestMergeable(user);
        }

        public void Moderate(IUser user) {
        }

        public void LoggingIn(string userNameOrEmail, string password) {
        }

        public void LogInFailed(string userNameOrEmail, string password) {
        }

        private void MergeUserToClosestMergeable(IUser user) {
            var userPart = user.ContentItem.As<UserPart>();
            if (userPart.LastLoginUtc != null) return;
            if (userPart.EmailStatus == UserStatus.Pending || userPart.RegistrationStatus == UserStatus.Pending) return;

            var closestUser = _orchardOpenAuthWebSecurity.GetClosestMergeableKnownUser(userPart);
            if (closestUser != null && closestUser.UserName != user.UserName) {
                closestUser.ContentItem.As<UserPart>().Password = userPart.Password;
                closestUser.ContentItem.As<UserPart>().PasswordFormat = userPart.PasswordFormat;
                closestUser.ContentItem.As<UserPart>().PasswordSalt = userPart.PasswordSalt;
                closestUser.ContentItem.As<UserPart>().UserName = userPart.UserName;
                closestUser.ContentItem.As<UserPart>().NormalizedUserName = userPart.NormalizedUserName;
                closestUser.ContentItem.As<UserPart>().HashAlgorithm = userPart.HashAlgorithm;
                _contentManager.Destroy(userPart.ContentItem);
                _notifier.Information(T("Your account has been merged with your previous account."));
            }
        }
    }
}