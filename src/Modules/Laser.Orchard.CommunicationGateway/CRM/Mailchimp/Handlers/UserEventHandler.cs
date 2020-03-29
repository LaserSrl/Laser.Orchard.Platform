using Laser.Orchard.CommunicationGateway.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.Mailchimp.Services;
using Laser.Orchard.Policy.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.Users.Events;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Handlers {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class UserEventHandler : IUserEventHandler {
        private IMailchimpApiService _apiService;
        private readonly IMailchimpService _service;
        private readonly ITransactionManager _transaction;
        private readonly IWorkContextAccessor _workContext;
        private readonly INotifier _notifier;

        public UserEventHandler(IMailchimpApiService apiService,
            IMailchimpService service,
            ITransactionManager transaction,
            IWorkContextAccessor workContext,
            INotifier notifier) {
            _apiService = apiService;
            _service = service;
            _transaction = transaction;
            _workContext = workContext;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

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
                if (part.Subscription.Audience == null || user.As<UserPart>().EmailStatus != UserStatus.Approved) return;
                var settings = part.Settings.GetModel<MailchimpSubscriptionPartSettings>();
                try {
                    _service.CheckAcceptedPolicy(part);
                }
                catch (MissingPoliciesException ex) {
                    _notifier.Add(NotifyType.Warning, T("Wait... it seems you have not accepted all our required policies. Your account has been verified, but your email have not been subscribed. Please, retry after having accepted our policies."));
                    return;
                }
                if (!_apiService.TryUpdateSubscription(part)) {
                    if (settings.NotifySubscriptionResult || AdminFilter.IsApplied(_workContext.GetContext().HttpContext.Request.RequestContext)) {
                        _notifier.Error(T("Oops! We are currently experienced a problem during your email subscription. Please, retry later."));
                    }
                }
                else {
                    if (settings.NotifySubscriptionResult || AdminFilter.IsApplied(_workContext.GetContext().HttpContext.Request.RequestContext)) {
                        _notifier.Information(T("Nice to meet you! Your subscription has been accepted."));
                    }
                }
            }
        }
    }
}