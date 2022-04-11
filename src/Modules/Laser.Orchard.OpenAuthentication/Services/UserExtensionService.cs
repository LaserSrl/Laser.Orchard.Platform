using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Users.Services;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Services;
using Orchard.Messaging.Services;
using Orchard.Environment.Configuration;
using Orchard.DisplayManagement;
using Orchard.Settings;
using Orchard.Users.Models;
using System;
using Orchard;
using JetBrains.Annotations;

namespace Laser.Orchard.OpenAuthentication.Services {
    [OrchardSuppressDependency("Orchard.Users.Services.UserService")]
    [UsedImplicitly]
    public class UserExtensionService : UserService , IUserService {
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardservices;
        private readonly IUserProviderServices _userProviderService;

        public UserExtensionService(
            IContentManager contentManager,
            IMembershipService membershipService,
            IClock clock,
            IMessageService messageService,
            ShellSettings shellSettings,
            IEncryptionService encryptionService,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay,
            ISiteService siteService,
            IOrchardServices orchardServices,
            IUserProviderServices userProviderService,
            IPasswordHistoryService passwordHistoryService
            )
            : base(contentManager, membershipService, clock, messageService, shellSettings, encryptionService, shapeFactory, shapeDisplay, siteService, passwordHistoryService) {
            _contentManager = contentManager;
            _orchardservices = orchardServices;
            _userProviderService = userProviderService;
        }

        public new bool VerifyUserUnicity(string userName, string email) {
            //string normalizedUserName = userName.ToLowerInvariant();
            //if (_contentManager.Query<UserPart, UserPartRecord>()
            //                       .Where(user =>
            //                              user.NormalizedUserName == normalizedUserName ||
            //                              user.Email == email)
            //                       .List().Any()) {
            //    return false;
            //}

            if (CheckUsernameUnicity(userName)) {
                return CheckEmailUnicityInLocalAccounts(email);
            } else {
                return false;
            }
        }

        public new bool VerifyUserUnicity(int id, string userName, string email) {
            //string normalizedUserName = userName.ToLowerInvariant();
            //if (_contentManager.Query<UserPart, UserPartRecord>()
            //                       .Where(user =>
            //                              user.NormalizedUserName == normalizedUserName ||
            //                              user.Email == email)
            //                       .List().Any(user => user.Id != id)) {
            //    return false;
            //}
            //return true;

            if (CheckUsernameUnicity(userName, id)) {
                if (_userProviderService.Get(id).Count() > 0) {
                    // account OAuth
                    return true;
                } else {
                    // account locale
                    return CheckEmailUnicityInLocalAccounts(email, id);
                }
            } else {
                return false;
            }
        }

        private bool CheckUsernameUnicity(string userName, int id = 0) {
            string normalizedUserName = userName.ToLowerInvariant();
            var count = _contentManager.Query<UserPart, UserPartRecord>()
                                   .Where(user =>
                                          user.NormalizedUserName == normalizedUserName
                                          && user.Id != id)
                                   .Count();
            return count == 0;
        }

        private bool CheckEmailUnicityInLocalAccounts(string email, int id = 0) {
            bool result = true;
            //var accounts = _contentManager.Query<UserPart, UserPartRecord>()
            //           .Where(user => string.IsNullOrWhiteSpace(user.Email) == false && user.Id != id && user.Email == email)
            //           .List();
            if (string.IsNullOrWhiteSpace(email) == false) {
                var accounts = _contentManager.Query<UserPart, UserPartRecord>()
                           .Where(user => user.Id != id && user.Email == email)
                           .List();
                foreach (var account in accounts) {
                    if (_userProviderService.Get(account.Id).Count() == 0) {
                        // account locale (no OAuth)
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }

        public new bool SendLostPasswordEmail(string usernameOrEmail, Func<string, string> createUrl) {
            // ricava lo username (univoco) dell'utenza locale (no OAuth) e lo passa alla classe padre
            string localUsername = "";
            var lowerName = usernameOrEmail.ToLowerInvariant();
            var accounts = _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName || u.Email == lowerName).List();
            foreach (var account in accounts) {
                if (_userProviderService.Get(account.Id).Count() == 0) {
                    // account locale (no OAuth)
                    localUsername = account.UserName;
                    break;
                }
            }
            return base.SendLostPasswordEmail(localUsername, createUrl);
        }
    }
}