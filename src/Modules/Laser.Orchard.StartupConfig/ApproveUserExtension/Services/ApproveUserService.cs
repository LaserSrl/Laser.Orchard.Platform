using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Users;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard.UI.Notify;
using Orchard.Users.Events;

namespace Laser.Orchard.StartupConfig.ApproveUserExtension.Services {
    public interface IApproveUserService : IDependency {
        void Approve(ContentItem contentItem);
        void Disable(ContentItem contentItem);
    }

    public class ApproveUserService : IApproveUserService {

        private readonly IUserEventHandler _userEventHandlers;
        public IOrchardServices Services { get; set; }


        public ApproveUserService(
           IUserEventHandler userEventHandlers,
           IOrchardServices services) {

            _userEventHandlers = userEventHandlers;
            Services = services;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }


        public void Approve(ContentItem contentItem) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return;

            var user = Services.ContentManager.Get<IUser>(contentItem.Id);

            if (user == null)
                return;

            user.As<UserPart>().RegistrationStatus = UserStatus.Approved;
            Services.Notifier.Information(T("User {0} approved", user.UserName));
            _userEventHandlers.Approved(user);
        }

        public void Disable(ContentItem contentItem) {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return;

            var user = Services.ContentManager.Get<IUser>(contentItem.Id);

            if (user == null)
                return;

            user.As<UserPart>().RegistrationStatus = UserStatus.Pending;
            Services.Notifier.Information(T("User {0} disabled", user.UserName));
            _userEventHandlers.Moderate(user);
        }
    }
}