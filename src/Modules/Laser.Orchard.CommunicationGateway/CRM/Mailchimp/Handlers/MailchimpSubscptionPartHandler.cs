using Laser.Orchard.CommunicationGateway.CRM.Mailchimp;
using Laser.Orchard.CommunicationGateway.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.Mailchimp.Services;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.UsersExtensions.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Settings.Metadata;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Users.Events;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Handlers {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSubscptionPartHandler : ContentHandler {
        private IMailchimpApiService _apiService;
        private readonly IMailchimpService _service;
        private readonly IWorkContextAccessor _workContext;
        private readonly ITransactionManager _transaction;
        private readonly INotifier _notifier;
        private string _subscrptionId;
        private bool _serverUpdated = false;
        private bool _modelIsValid = true;

        public Localizer T { get; set; }


        public MailchimpSubscptionPartHandler(
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


            OnUpdating<MailchimpSubscriptionPart>((context, part) => {
                _subscrptionId = part.Subscription.Subscribed ? part.Subscription.Audience.Identifier : "(undefined)";
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
                    _service.CheckAcceptedPolicy(context.ContentItem.As<MailchimpSubscriptionPart>(), context.ContentItem.As<UserRegistrationPolicyPart>());
                }
                catch (MissingPoliciesException ex) {
                    context.Updater.AddModelError("MissingPolicies", T("You have to accepted all required policies in order to subscribe to the newsletter."));
                    _modelIsValid = false;
                }
                catch (Exception ex) {
                    context.Updater.AddModelError("GenericPolicies", T("You have to accepted all required policies in order to subscribe to the newsletter."));
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
            if (_subscrptionId != (part.Subscription.Subscribed ? part.Subscription.Audience.Identifier : "(undefined)")) {
                return _apiService.TryUpdateSubscription(part);
            }
            else {
                return false;
            }

        }
    }
}