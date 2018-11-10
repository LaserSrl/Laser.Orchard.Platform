using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Workflows.Services;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Laser.Orchard.TemplateManagement.Controllers {
    [OrchardFeature("Laser.Orchard.WebTracking")]
    public class WebTrackingApiController : ApiController {
        private readonly IContentManager _contentManager;
        private readonly IWorkflowManager _workflowManager;
        public WebTrackingApiController(IWorkflowManager workflowManager, IContentManager contentManager) {
            _workflowManager = workflowManager;
            _contentManager = contentManager;
        }
        [OutputCache(NoStore = true, Duration = 0)]
        public HttpResponseMessage Get() {
            var tokens = new Dictionary<string, object>();
            var site = _contentManager.Get(1);
            tokens.Add("Content", site);
            // add content of query string in tokens of the workflow
            var queryString = Request.GetQueryNameValuePairs();
            foreach(var item in queryString) {
                // add only if token does not exist yet, so "Content" cannot be overwritten
                if(tokens.ContainsKey(item.Key) == false) {
                    tokens.Add(item.Key, item.Value);
                }
            }
            _workflowManager.TriggerEvent("WebTrackingEvent", site, () => tokens);
            // return an invisible image
            var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            result.Headers.Clear();
            var content = new ByteArrayContent(File.ReadAllBytes(HttpContext.Current.Server.MapPath("~/Modules/Laser.Orchard.TemplateManagement/Styles/1x1.png")));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            result.Content = content;
            return result;
        }
    }
}