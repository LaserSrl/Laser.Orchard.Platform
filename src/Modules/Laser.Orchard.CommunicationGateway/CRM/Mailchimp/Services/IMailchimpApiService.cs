using Laser.Orchard.CommunicationGateway.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.Mailchimp.ViewModels;
using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Services {
    public interface IMailchimpApiService : IDependency {
        List<Audience> Audiences();
        Audience Audience(string id);
        bool TryUpdateSubscription(MailchimpSubscriptionPart part);
    }
}
