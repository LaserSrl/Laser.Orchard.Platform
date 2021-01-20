using Laser.Orchard.WebServices.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Workflows.Activities;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace Laser.Orchard.WebServices.Controllers {
    [OrchardFeature("Laser.Orchard.CustomRestApi")]
    [OutputCache(NoStore = true, Duration = 0)]
    public class CustomRESTApiController : ApiController {
        private readonly IWorkflowManager _workflowManager;
        private readonly IContentManager _contentManager;

        public CustomRESTApiController(
            IWorkflowManager workflowManager,
            IContentManager contentManager) {

            _workflowManager = workflowManager;
            _contentManager = contentManager;

            Log = NullLogger.Instance;
        }

        public ILogger Log { get; set; }

        public IHttpActionResult Get(string actionName, int contentId = 0) {
            // CRUD: read
            var msg = ActionValidation("get", actionName);
            // if validation already has a message to send, it means we should not
            // execute this method, but rather return that.
            if (msg == null) {
                // TODO:
            }
            return ResponseMessage(msg);
        }
        public IHttpActionResult Post(string actionName, int contentId = 0) {
            // CRUD: create
            var msg = ActionValidation("post", actionName);
            // if validation already has a message to send, it means we should not
            // execute this method, but rather return that.
            if (msg == null) {
                // TODO:
                msg = SignalInvocation("post", actionName, contentId);
            }
            return ResponseMessage(msg);
        }
        public IHttpActionResult Put(string actionName, int contentId = 0) {
            // CRUD: Update/Replace
            var msg = ActionValidation("put", actionName);
            // if validation already has a message to send, it means we should not
            // execute this method, but rather return that.
            if (msg == null) {
                // TODO:
            }
            return ResponseMessage(msg);
        }
        public IHttpActionResult Patch (string actionName, int contentId = 0) {
            // CRUD: Update/Modify
            var msg = ActionValidation("patch", actionName);
            // if validation already has a message to send, it means we should not
            // execute this method, but rather return that.
            if (msg == null) {
                // TODO:
            }
            return ResponseMessage(msg);
        }
        public IHttpActionResult Delete(string actionName, int contentId = 0) {
            // CRUD: Delete
            var msg = ActionValidation("delete", actionName);
            // if validation already has a message to send, it means we should not
            // execute this method, but rather return that.
            if (msg == null) {
                // TODO:
            }
            return ResponseMessage(msg);
        }

        public IHttpActionResult Head(string actionName, int contentId = 0) {
            // CRUD: read
            var msg = ActionValidation("head", actionName);
            // if validation already has a message to send, it means we should not
            // execute this method, but rather return that.
            if (msg == null) {
                // TODO:
                // The response from this shoud have and empty body, but the 
                // headers should be identical to those we would send with a GET
            }
            return ResponseMessage(msg);
        }
        public IHttpActionResult Options(string actionName) {
            var msg = ActionValidation("options", actionName);
            // if validation already has a message to send, it means we should not
            // execute this method, but rather return that.
            if (msg == null) {
                msg = new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("")
                };

                msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                foreach (var method in AllowedMethods(actionName)) {
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
            return ResponseMessage(msg);
        }


        private HttpResponseMessage SignalInvocation(string verb, string actionName, int contentId) {
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
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            //TODO
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
            // TODO
            if ("delete".Equals(verb, StringComparison.InvariantCultureIgnoreCase)) {
                return false;
            }
            return true;
        }
        private bool MethodIsAllowed(string verb, string actionName) {
            // TODO
            if ("patch".Equals(verb, StringComparison.InvariantCultureIgnoreCase)) {
                return false;
            }
            return true;
        }
        private string[] AllowedMethods(string actionName) {
            // TODO
            return new string[] {
                "GET", "POST", "PUT", "PATCH", "DELETE"
            };
        }
    }
}