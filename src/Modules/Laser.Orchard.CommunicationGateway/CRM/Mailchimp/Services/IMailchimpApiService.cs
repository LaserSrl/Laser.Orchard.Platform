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
using System.Net.Http;
using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Services;
using System.Web.Mvc;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Services {
    public interface IMailchimpApiService : IDependency {
        List<Audience> Audiences();
        Audience Audience(string id);
        bool TryUpdateSubscription(MailchimpSubscriptionPart part);
        bool TryApiCall(HttpVerbs httpVerb, RequestTypes urlType, IDictionary<string, string> urlTokens, object bodyRequest, HttpResponseMessage response);
        Dictionary<RequestTypes, string> RequestTypeUrls();
    }
}
