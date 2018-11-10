using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Fields;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.Mobile.Controllers {
    [WebApiKeyFilter(true)]
    [OrchardFeature("Laser.Orchard.Mobile.ExternalPush")]
    public class ExternalPushController : ApiController {
        private readonly IPushGatewayService _pushGatewayService;
        private readonly IOrchardServices _orchardServices;
        private readonly IScheduledTaskManager _taskManager;
        public ILogger Logger { get; set; }

        public class PushRequest {
            public string Text { get; set; }
            public string DevType { get; set; }
            public bool Prod { get; set; }
            public string ExternalUrl { get; set; }
        }

        public ExternalPushController(IPushGatewayService pushGatewayService, IOrchardServices orchardServices, IScheduledTaskManager taskManager) {
            _pushGatewayService = pushGatewayService;
            _orchardServices = orchardServices;
            _taskManager = taskManager;
            Logger = NullLogger.Instance;
        }

        public HttpResponseMessage Post(PushRequest req) {
            HttpResponseMessage message = null;
            try {
                // schedule the sending of the push notification
                var ci = _orchardServices.ContentManager.Create("BackgroundPush");
                var part = ci.As<MobilePushPart>();
                part.DevicePush = req.DevType;
                part.PushSent = false;
                part.TestPush = !req.Prod;
                part.TestPushToDevice = false;
                part.TextPush = req.Text;
                part.ToPush = true;
                part.UseRecipientList = false;
                // set external url on a dedicated field on this content item
                var urlField = ci.Parts.FirstOrDefault(x => x.PartDefinition.Name == "BackgroundPush").Fields.FirstOrDefault(x => x.Name == "ExternalUrl");
                (urlField as TextField).Value = req.ExternalUrl;
                // force publish of content item to start scheduled task that will send push notifications
                ci.VersionRecord.Published = false;
                _orchardServices.ContentManager.Publish(ci);
                message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                message.Content = new StringContent("OK");
            }
            catch(Exception ex) {
                Logger.Error(ex, "ExternalPushController - Error starting asynchronous thread to send push notifications.");
                message = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
                message.Content = new StringContent(ex.Message);
            }
            return message;
        }
    }
}