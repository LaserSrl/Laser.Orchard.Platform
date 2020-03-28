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
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.ViewModels;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Services {
    public interface IMailchimpService : IDependency {
        string DecryptApiKey();
        string CryptApiKey(string apikey);
        void CheckAcceptedPolicy(MailchimpSubscriptionPart part);

    }
}
