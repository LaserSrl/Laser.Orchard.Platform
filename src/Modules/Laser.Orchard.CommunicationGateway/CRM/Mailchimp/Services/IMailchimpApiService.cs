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
        bool TryUpdateSubscription(MailchimpSubscriptionPart part);
        bool TryApiCall(HttpVerbs httpVerb, string url, JObject bodyRequest, Func<HttpVerbs, string, JObject, HttpResponseMessage, bool> ErrorHandler, ref string result);
        bool ErrorHandlerDefault(HttpVerbs httpVerb, string requestUrl, JObject bodyRequest, HttpResponseMessage response);
        bool ErrorHandlerDelete(HttpVerbs httpVerb, string requestUrl, JObject bodyRequest, HttpResponseMessage response);
        bool ErrorHandlerGet(HttpVerbs httpVerb, string requestUrl, JObject bodyRequest, HttpResponseMessage response);
        List<RequestTypeInfo> GetRequestTypes();
        bool IsUserRegister(MailchimpSubscriptionPart part);
    }
}