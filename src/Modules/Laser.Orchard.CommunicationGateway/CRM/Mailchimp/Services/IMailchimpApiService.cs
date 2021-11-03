using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.ViewModels;
using Newtonsoft.Json.Linq;
using Orchard;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Mvc;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Services {
    public interface IMailchimpApiService : IDependency {
        string GetBaseUrl();
        List<Audience> Audiences();
        Audience Audience(string id);
        bool TryUpdateSubscription(MailchimpSubscriptionPart part, bool isUserCreation = false);
        bool TryApiCall(HttpVerbs httpVerb, string url, JObject bodyRequest, ref string result);

        List<RequestTypeInfo> GetRequestTypes();
        bool IsUserRegistered(MailchimpSubscriptionPart part);
    }
}