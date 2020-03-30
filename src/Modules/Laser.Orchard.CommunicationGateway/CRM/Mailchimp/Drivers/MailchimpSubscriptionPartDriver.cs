using Laser.Orchard.CommunicationGateway.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.Mailchimp.Services;
using Laser.Orchard.CommunicationGateway.Mailchimp.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions;
using Orchard.Users.Models;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Drivers {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSubscriptionPartDriver : ContentPartDriver<MailchimpSubscriptionPart> {
        private readonly IMailchimpApiService _service;

        public MailchimpSubscriptionPartDriver(IMailchimpApiService service) {
            _service = service;
        }
        protected override string Prefix => "MailchimpSubscription";

        protected override DriverResult Display(MailchimpSubscriptionPart part, string displayType, dynamic shapeHelper) {
            return null;
        }
        protected override DriverResult Editor(MailchimpSubscriptionPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }
        protected override DriverResult Editor(MailchimpSubscriptionPart part, IUpdateModel updater, dynamic shapeHelper) {
            var settings = part.Settings.GetModel<MailchimpSubscriptionPartSettings>();
            SelectableAudience selectableAudience ;
            if (string.IsNullOrWhiteSpace(settings.AudienceId)) {
                selectableAudience = new SelectableAudience();
            }
            else {
                var subscription = part.Subscription;
                if (subscription.Audience == null || settings.AudienceId != subscription.Audience.Identifier) {
                    var audience = _service.Audience(settings.AudienceId);
                    selectableAudience = new SelectableAudience {
                        Audience = new Audience { Identifier = settings.AudienceId, Name = audience.Name },
                        Selected = !part.Is<UserPart>(),
                        RequiredPolicies = settings.PolicyTextReferences
                    };
                }
                else {
                    selectableAudience = new SelectableAudience {
                        Audience = subscription.Audience,
                        Selected = subscription.Subscribed,
                        RequiredPolicies = settings.PolicyTextReferences
                    };
                }
                if (updater != null) {
                    if (updater.TryUpdateModel(selectableAudience, Prefix, null, null)) {
                        part.Subscription = new Subscription {
                            Audience = selectableAudience.Audience,
                            Subscribed = selectableAudience.Selected
                        };
                    }
                }
            }
            return ContentShape("Parts_MailchimpSubscription_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/Mailchimp/MailchimpSubscription_Edit", Model: selectableAudience, Prefix: Prefix));
        }
    }
}