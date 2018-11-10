using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.UI.Notify;
using System;

namespace Laser.Orchard.Mobile.Handlers {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class MobilePushPartHandler : ContentHandler {
        private readonly INotifier _notifier;
        private readonly IScheduledTaskManager _taskManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IPushGatewayService _pushGatewayService;
        public Localizer T { get; set; }

        public MobilePushPartHandler(IRepository<MobilePushPartRecord> repository, INotifier notifier, IScheduledTaskManager taskManager, IOrchardServices orchardServices, IPushGatewayService pushGatewayService) {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _notifier = notifier;
            _orchardServices = orchardServices;
            _pushGatewayService = pushGatewayService;
            _taskManager = taskManager;
            Filters.Add(StorageFilter.For(repository));

            OnUpdated<MobilePushPart>((context, part) => {
                if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "submit.PushTest") {
                    // Invio Push di Test
                    _pushGatewayService.PublishedPushEventTest(part.ContentItem);
                }

                if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "submit.PushContact") {
                    // invia la push al contact selezionato
                    string contactTitle = "";
                    string aux = _orchardServices.WorkContext.HttpContext.Request.Form["contact-to-push"];
                    // rimuove il numero di device racchiuso tra parentesi per ricavare il nome del contact
                    int idx = aux.LastIndexOf(" (");
                    if (idx > 0) {
                        contactTitle = aux.Substring(0, idx);
                    } else {
                        contactTitle = aux;
                    }
                    // invia la push
                    if ((string.IsNullOrWhiteSpace(contactTitle) == false) && (string.IsNullOrWhiteSpace(part.TextPush) == false)) {
                        _pushGatewayService.SendPushToContact(part.ContentItem, contactTitle);
                        _notifier.Information(T("Push message will be sent to contact {0} in a minute.", contactTitle));
                    }
                }
            });

            OnPublished<MobilePushPart>((context, part) => {
                try {
                    if ((part.ToPush == true) && (part.PushSent == false)) {
                        _taskManager.CreateTask("Laser.Orchard.PushNotification.Task", DateTime.UtcNow.AddMinutes(-1), part.ContentItem);
                        part.PushSent = true;
                    }
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error starting asynchronous thread to send push notifications.");
                }
            });
        }
    }
}

