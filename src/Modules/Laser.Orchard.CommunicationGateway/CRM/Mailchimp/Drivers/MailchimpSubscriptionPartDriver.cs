using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Services;
using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Users.Models;
using System.Linq;
using Orchard.Tokens;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Drivers {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSubscriptionPartDriver : ContentPartDriver<MailchimpSubscriptionPart> {
        private readonly IMailchimpApiService _service;
        private readonly ITokenizer _tokenizer;


        public MailchimpSubscriptionPartDriver(IMailchimpApiService service, ITokenizer tokenizer) {
            _service = service;
            _tokenizer = tokenizer;
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
            var audienceId = _tokenizer.Replace(settings.AudienceId, new { Content = part.ContentItem });
            if (string.IsNullOrWhiteSpace(audienceId)) {
                selectableAudience = new SelectableAudience();
            }
            else {
                var subscription = part.Subscription;
                if (subscription.Audience == null || audienceId != subscription.Audience?.Identifier) {
                    var audience = _service.Audience(audienceId);
                    selectableAudience = new SelectableAudience {
                        Audience = new Audience { Identifier = audienceId, Name = audience.Name },
                        Selected = !part.Is<UserPart>(),
                        RequiredPolicies = settings.PolicyTextReferencesToArray()
                    };
                }
                else {
                    selectableAudience = new SelectableAudience {
                        Audience = subscription.Audience,
                        Selected = subscription.Subscribed,
                        RequiredPolicies = settings.PolicyTextReferencesToArray()
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