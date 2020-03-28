using Laser.Orchard.CommunicationGateway.Mailchimp.ViewModels;
using Laser.Orchard.CommunicationGateway.Mailchimp.Models;
using Orchard;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.Security;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Services {
    public interface IMailchimpApiService : IDependency {
        List<Audience> Audiences();
        Audience Audience(string id);
        bool TryUpdateSubscription(MailchimpSubscriptionPart part);
    }
}
