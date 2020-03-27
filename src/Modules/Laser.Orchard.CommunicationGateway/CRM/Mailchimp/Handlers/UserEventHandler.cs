using Laser.Orchard.CommunicationGateway.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.Mailchimp.Services;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Handlers {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class UserEventHandler : IUserEventHandler {
        private IMailchimpService _service;

        public UserEventHandler(IMailchimpService service) {
            _service = service;
        }

        public void AccessDenied(IUser user) {

        }

        public void Approved(IUser user) {
            UpdateSubscrption(user);
          }

        public void ChangedPassword(IUser user) {

        }

        public void ConfirmedEmail(IUser user) {
            UpdateSubscrption(user);
        }

        public void Created(UserContext context) {

        }

        public void Creating(UserContext context) {

        }

        public void LoggedIn(IUser user) {

        }

        public void LoggedOut(IUser user) {

        }

        public void LoggingIn(string userNameOrEmail, string password) {

        }

        public void LogInFailed(string userNameOrEmail, string password) {

        }

        public void Moderate(IUser user) {

        }

        public void SentChallengeEmail(IUser user) {

        }

        private void UpdateSubscrption(IUser user) {
            var part = user.As<MailchimpSubscriptionPart>();
            if (part != null) {
                // When a User is Created a Published is improperly called to early and the part is not Updated
                // So I skip this step
                if (part.Subscription.Audience == null || user.As<UserPart>().RegistrationStatus != UserStatus.Approved || user.As<UserPart>().EmailStatus != UserStatus.Approved) return;
                _service.TryVerifyPoliciesAndUpdateSubscription(part, user);
            }
        }
    }
}