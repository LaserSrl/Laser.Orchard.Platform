using Laser.Orchard.CommunicationGateway.CRM.Mailchimp;
using Laser.Orchard.CommunicationGateway.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.Mailchimp.Services;

using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using System;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Handlers {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSubscriptionPartHandler : ContentHandler {
        private IMailchimpApiService _apiService;
        private readonly IMailchimpService _service;
        private readonly IWorkContextAccessor _workContext;
        private readonly ITransactionManager _transaction;
        private readonly INotifier _notifier;
        private string _subscriptionId;
        private bool _serverUpdated = false;
        private bool _modelIsValid = true;

        public Localizer T { get; set; }


        public MailchimpSubscriptionPartHandler(
            IMailchimpApiService apiService,
            IMailchimpService service,
            IContentManager contentManager,
            IWorkContextAccessor workContext,
            ITransactionManager transaction,
            INotifier notifier) {
            _apiService = apiService;
            _service = service;
            _workContext = workContext;
            _transaction = transaction;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _subscriptionId = "(undefined)";

            OnUpdating<MailchimpSubscriptionPart>((context, part) => {
                if (part.Subscription == null || part.Subscription.Audience == null) {
                    _subscriptionId = "(undefined)";
                }
                else {
                    _subscriptionId = part.Subscription.Subscribed ? part.Subscription.Audience.Identifier : "(undefined)";
                }
            });

            OnUpdated<MailchimpSubscriptionPart>((context, part) => {
                if (!_serverUpdated && _modelIsValid) {
                    _serverUpdated = TryUpdateSubscription(part);
                }
            });

            OnPublished<MailchimpSubscriptionPart>((context, part) => {
                if (!_serverUpdated && _modelIsValid) {
                    _serverUpdated = TryUpdateSubscription(part);
                }
            });
        }
        protected override void UpdateEditorShape(UpdateEditorContext context) {
            base.UpdateEditorShape(context);
            if (context.ContentItem.As<MailchimpSubscriptionPart>() != null) {
                try {
                    _service.CheckAcceptedPolicy(context.ContentItem.As<MailchimpSubscriptionPart>());
                }
                catch (MissingPoliciesException ex) {
                    context.Updater.AddModelError("MissingPolicies", T("You have to accept all required policies in order to subscribe to the newsletter."));
                    _modelIsValid = false;
                }
                catch (Exception ex) {
                    context.Updater.AddModelError("GenericPolicies", T("You have to accept all required policies in order to subscribe to the newsletter."));
                    context.Logger.Log(LogLevel.Error, ex, "CheckAcceptedPolicy throws an error.", null);
                    _modelIsValid = false;
                }
            }

        }

        private bool TryUpdateSubscription(MailchimpSubscriptionPart part) {
            // When a User is Created a Published is improperly called to early and the part is not Updated
            // So I skip this step
            if (part.Subscription.Audience == null) return false;
            if (part.As<UserPart>() != null && part.As<UserPart>().EmailStatus != UserStatus.Approved) {
                // A registered user with an EmailStatus not Approved should not Update Mailchimp member
                return false;
            }
            if (!part.IsPublished()) {
                return false;
            }
            // check if subscriptions have changed during the publish process:
            // if changed it fires an update over Mailchimp servers
            if (_subscriptionId != (part.Subscription.Subscribed ? part.Subscription.Audience.Identifier : "(undefined)")) {
                var settings = part.Settings.GetModel<MailchimpSubscriptionPartSettings>();
                if (!_apiService.TryUpdateSubscription(part)) {
                    if (settings.NotifySubscriptionResult || AdminFilter.IsApplied(_workContext.GetContext().HttpContext.Request.RequestContext)) {
                        _notifier.Error(T("Oops! We have experienced a problem during your email subscription. Please, retry later."));
                    }
                    return false;
                }
                else {
                    if (settings.NotifySubscriptionResult || AdminFilter.IsApplied(_workContext.GetContext().HttpContext.Request.RequestContext)) {
                        _notifier.Information(T("Nice to meet you! Your subscription has been accepted."));
                    }
                    return true;
                }
            }
            else {
                return false;
            }

        }
    }
}