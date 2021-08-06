using Orchard.Environment.Extensions;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Models {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSubscriptionPartSettings {
        public string PutPayload { get; set; }
        public string MemberEmail { get; set; }
        public string AudienceId { get; set; }
        public string PolicyTextReferences { get; set; }
        public bool NotifySubscriptionResult { get; set; }

        public string[] PolicyTextReferencesToArray() {
            if (!string.IsNullOrWhiteSpace(this.PolicyTextReferences)) {
                return this.PolicyTextReferences.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            }
            else {
                return new string[] { };
            }
        }
    }
}