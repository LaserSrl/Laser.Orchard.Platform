using Orchard.Environment.Extensions;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.ViewModels {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSiteSettingsEdit {

        public string NewApiKey { get; set; }
        public string ApiKey { get; set; }
        public string DefaultAudience { get; set; }
    }
}