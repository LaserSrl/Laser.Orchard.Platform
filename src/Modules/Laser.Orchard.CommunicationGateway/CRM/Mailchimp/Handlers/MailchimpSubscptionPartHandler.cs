using Laser.Orchard.CommunicationGateway.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.Mailchimp.Services;
using Laser.Orchard.Policy.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Settings.Metadata;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Handlers {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSubscptionPartHandler : ContentHandler {
        private IMailchimpService _service;
        private readonly IWorkContextAccessor _workContext;
        private string _subscrptionId;
        private bool _serverUpdated = false;

        public MailchimpSubscptionPartHandler(IMailchimpService service, IContentManager contentManager, IWorkContextAccessor workContext) {
            _service = service;
            _workContext = workContext;
            OnUpdating<MailchimpSubscriptionPart>((context, part) => {
                _subscrptionId = part.Subscription.Subscribed ? part.Subscription.Audience.Identifier : "(undefined)";
            });

            OnUpdated<MailchimpSubscriptionPart>((context, part) => {
                if (!_serverUpdated) {
                    _serverUpdated = TryUpdateSubscription(part);
                }
            });

            OnPublished<MailchimpSubscriptionPart>((context, part) => {
                if (!_serverUpdated) {
                    _serverUpdated = TryUpdateSubscription(part);
                }
            });
        }

        private bool TryUpdateSubscription(MailchimpSubscriptionPart part) {
            // When a User is Created a Published is improperly called to early and the part is not Updated
            // So I skip this step
            if (part.Subscription.Audience == null) return false;
            if (part.As<UserPart>() != null && part.As<UserPart>().EmailStatus != UserStatus.Approved) {
                // A registered user with an EmailStatus not Approved should not Update Mailchimp member
                return false;
            }
            else if (!part.IsPublished()) {
                return false;
            }
            // check if subscriptions have changed during the publish process:
            // if changed it fires an update over Mailchimp servers
            if (_subscrptionId != (part.Subscription.Subscribed ? part.Subscription.Audience.Identifier : "(undefined)")) {
                return _service.TryVerifyPoliciesAndUpdateSubscription(part, part.As<UserPart>() ?? _workContext.GetContext().CurrentUser);
            }
            else {
                return false;
            }

        }
    }
}