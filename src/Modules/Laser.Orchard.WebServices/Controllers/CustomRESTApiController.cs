using Laser.Orchard.WebServices.Helpers;
using Laser.Orchard.WebServices.Models;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.Workflows.Activities;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Mvc;

namespace Laser.Orchard.WebServices.Controllers {
    [OrchardFeature("Laser.Orchard.CustomRestApi")]
    [OutputCache(NoStore = true, Duration = 0)]
    public class CustomRESTApiController : ApiController {
        private readonly IWorkflowManager _workflowManager;
        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public CustomRESTApiController (
            IWorkflowManager workflowManager,
            IContentManager contentManager, 
            ICacheManager cacheManager, 
            ISignals signals,
            ISiteService siteService) {

            _workflowManager = workflowManager;
            _contentManager = contentManager;
            _cacheManager = cacheManager;
            _signals = signals;
            _siteService = siteService;

            Log = NullLogger.Instance;

            _settingsDictionary = GetAllSettings();
        }

        private Dictionary<string, RestApiAction> _settingsDictionary;

        public ILogger Log { get; set; }

        public IHttpActionResult Get(string customActionName, int contentId = 0) {
            // CRUD: read
            var msg = ActionValidation("get", customActionName);
            // if validation already has a message to send, it means we should not
            // execute this method, but rather return that.
            if (msg == null) {
                // TODO:
                msg = SignalInvocation("get", customActionName, contentId);
            }
            if (msg == null) {
                // fallback
                msg = new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            return ResponseMessage(msg);
        }
        public IHttpActionResult Post(string customActionName, int contentId = 0) {
            // CRUD: create
            var msg = ActionValidation("post", customActionName);
            // if validation already has a message to send, it means we should not
            // execute this method, but rather return that.
            if (msg == null) {
                // TODO:
                msg = SignalInvocation("post", customActionName, contentId);
            }
            if (msg == null) {
                // fallback
                msg = new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            return ResponseMessage(msg);
        }
        public IHttpActionResult Put(string customActionName, int contentId = 0) {
            // CRUD: Update/Replace
            var msg = ActionValidation("put", customActionName);
            // if validation already has a message to send, it means we should not
            // execute this method, but rather return that.
            if (msg == null) {
                // TODO:
                msg = SignalInvocation("put", customActionName, contentId);
            }
            if (msg == null) {
                // fallback
                msg = new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            return ResponseMessage(msg);
        }
        public IHttpActionResult Patch (string customActionName, int contentId = 0) {
            // CRUD: Update/Modify
            var msg = ActionValidation("patch", customActionName);
            // if validation already has a message to send, it means we should not
            // execute this method, but rather return that.
            if (msg == null) {
                // TODO:
                msg = SignalInvocation("patch", customActionName, contentId);
            }
            if (msg == null) {
                // fallback
                msg = new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            return ResponseMessage(msg);
        }
        public IHttpActionResult Delete(string customActionName, int contentId = 0) {
            // CRUD: Delete
            var msg = ActionValidation("delete", customActionName);
            // if validation already has a message to send, it means we should not
            // execute this method, but rather return that.
            if (msg == null) {
                // TODO:
                msg = SignalInvocation("delete", customActionName, contentId);
            }
            if (msg == null) {
                // fallback
                msg = new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            return ResponseMessage(msg);
        }

        public IHttpActionResult Head(string customActionName, int contentId = 0) {
            // CRUD: read
            var msg = ActionValidation("head", customActionName);
            // if validation already has a message to send, it means we should not
            // execute this method, but rather return that.
            if (msg == null) {
                // TODO:
                // The response from this shoud have and empty body, but the 
                // headers should be identical to those we would send with a GET
                var resp = InnerSignalInvocation("get", customActionName, contentId);
                resp.Content = "";
                msg = resp.ToMessage();
            }
            if (msg == null) {
                // fallback
                msg = new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            return ResponseMessage(msg);
        }
        public IHttpActionResult Options(string customActionName) {
            var msg = ActionValidation("options", customActionName);
            // if validation already has a message to send, it means we should not
            // execute this method, but rather return that.
            if (msg == null) {
                msg = new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("")
                };

                msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                foreach (var method in AllowedMethods(customActionName)) {
                    msg.Content.Headers.Allow.Add(method);
                }

                // TODO: settings for CORS: Example headers for CORS
                // Next line would allow CORS (cross origin resource sharing).
                // Consider activating it based on a setting
                //httpResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                // Next line would tell the allowed verbs (especially for CORS)
                // The list of verbs for the given actionName could come from a setting:
                // if the verb is not allowed, calling it should return a 405
                //httpResponse.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, PATCH, DELETE");
            }
            if (msg == null) {
                // fallback
                msg = new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            return ResponseMessage(msg);
        }

        private RestApiResponse InnerSignalInvocation(string verb, string actionName, int contentId) {
            // we want what we return here to map to HttpResponseMessage
            RestApiResponse restResult = new RestApiResponse();
            var signalName = SignalName(verb, actionName);
            try {
                var content = _contentManager.Get(contentId);
                var tokens = new Dictionary<string, object> {
                    { "Content", content },
                    { "HttpVerb", verb },
                    { "RESTActionName", actionName },
                    { SignalActivity.SignalEventName, signalName },
                    { "RESTApiResponse", restResult }
                };
                _workflowManager
                    .TriggerEvent(SignalActivity.SignalEventName, content, () => tokens);
            } catch (Exception ex) {
                Log.Error("CustomApiController.SignalInvocation: " + Environment.NewLine
                    + "verb: " + verb + Environment.NewLine
                    + "actionName: " + actionName + Environment.NewLine
                    + "contentId: " + contentId.ToString() + Environment.NewLine
                    + "signalName: " + signalName + Environment.NewLine
                    + "Exception.Message: " + ex.Message + Environment.NewLine
                    + "Exception.StackTrace: " + ex.StackTrace);
                return new RestApiResponse(HttpStatusCode.InternalServerError);
            }
            return restResult;
        }
        private HttpResponseMessage SignalInvocation(string verb, string actionName, int contentId) {
            // we want what we return here to map to HttpResponseMessage
            RestApiResponse restResult = InnerSignalInvocation(verb, actionName, contentId);
            if (restResult == null) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            return restResult.ToMessage();
        }

        private string SignalName(string verb, string actionName) {
            return $"{verb.ToUpperInvariant()}_{actionName.ToUpperInvariant()}";
        }

        private HttpResponseMessage ActionValidation(string verb, string actionName) {
            // this method will return an HttpResponseMessage if there is any
            // validation issue.
            // TODO: a provider for these
            // TODO: a way to have "composite" actionNames, like 
            //   /foo/bar/stuff
            if (!ActionExists(verb, actionName)) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            if (!MethodIsAllowed(verb, actionName)) {
                var msg = new HttpResponseMessage(HttpStatusCode.MethodNotAllowed) {
                    Content = new StringContent("")
                };
                foreach (var method in AllowedMethods(actionName)) {
                    msg.Content.Headers.Allow.Add(method);
                }
                return msg;
            }
            // returning null tells callers that they should go ahead and provide
            // a response themselves
            return null;
        }

        private bool ActionExists(string verb, string actionName) {
            return _settingsDictionary.ContainsKey(actionName.ToUpperInvariant());
        }
        private bool MethodIsAllowed(string verb, string actionName) {

            return AllowedMethods(actionName)
                .Any(v => verb.Equals(v, StringComparison.InvariantCultureIgnoreCase));
        }
        private IEnumerable<string> AllowedMethods(string actionName) {
            if (_settingsDictionary.ContainsKey(actionName.ToUpperInvariant())) {
                return _settingsDictionary[actionName.ToUpperInvariant()].Verbs;
            }
            return new string[] { };
        }

        private Dictionary<string, RestApiAction> GetAllSettings() {
            return _cacheManager.Get(CustomRestApiHelper.SettingsCacheKey, true, context => {
                context.Monitor(_signals.When(CustomRestApiHelper.SettingsCacheKey));
                var part = _siteService.GetSiteSettings().As<CustomRestApiSiteSettingsPart>();
                var settings = new Dictionary<string, RestApiAction>();
                foreach (var item in part.GetActionsConfiguration()) {
                    settings.Add(item.Name.ToUpperInvariant(), item);
                }
                //TODO: improve this with providers injecting further configurations
                //TODO: handle duplicate actionNames somehow. Perhaps a priority of providers?
                return settings;
            });
        }
    }
}