using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Models;
using Orchard;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Services {
    public interface IMailchimpService : IDependency {
        string DecryptApiKey();
        string CryptApiKey(string apikey);
        string ComputeSubscriberHash(string input);
        void CheckAcceptedPolicy(MailchimpSubscriptionPart part);

    }
}
