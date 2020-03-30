using Laser.Orchard.CommunicationGateway.Mailchimp.Models;
using Orchard;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Services {
    public interface IMailchimpService : IDependency {
        string DecryptApiKey();
        string CryptApiKey(string apikey);
        void CheckAcceptedPolicy(MailchimpSubscriptionPart part);

    }
}
