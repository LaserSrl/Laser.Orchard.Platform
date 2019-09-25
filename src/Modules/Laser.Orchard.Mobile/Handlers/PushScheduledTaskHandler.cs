using Laser.Orchard.Mobile.Services;
using Laser.Orchard.CommunicationGateway.Services;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Laser.Orchard.Mobile.Models;

namespace Laser.Orchard.Mobile.Handlers {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushScheduledTaskHandler : IScheduledTaskHandler {
        private readonly IPushGatewayService _pushGatewayService;
        private readonly ICommunicationService _communicationService;
        private readonly IScheduledTaskManager _taskManager;
        private readonly IOrchardServices _orchardServices;
        private const string taskType = "Laser.Orchard.PushNotification.Task";

        public ILogger Logger { get; set; }

        public PushScheduledTaskHandler(IPushGatewayService pushGatewayService, ICommunicationService communicationService, IScheduledTaskManager taskManager, IOrchardServices orchardServices) {
            _pushGatewayService = pushGatewayService;
            _communicationService = communicationService;
            _taskManager = taskManager;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
        }

        public void Process(ScheduledTaskContext context) {
            try {
                if (context.Task.TaskType != taskType) {
                    return;
                }
                // esegue l'invio delle push
                var state = _pushGatewayService.PublishedPushEvent(context.Task.ContentItem);
                //verifica se rischedulare l'invio
                var pushSettings = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>();
                var runAgain = _communicationService.GetRunAgainNeeded(context.Task.ContentItem.Id, "push", state.Errors, state.CompletedIteration, pushSettings.MaxNumRetry);
                if (runAgain) {
                    // rischedula il task
                    // applica il valore di default (5) nel caso di setting non valorizzato
                    var delay = pushSettings.DelayMinutesBeforeRetry == 0 ? 5 : pushSettings.DelayMinutesBeforeRetry;
                    _taskManager.CreateTask(taskType, DateTime.UtcNow.AddMinutes(delay), context.Task.ContentItem);
                }
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error in PushScheduledTaskHandler. ContentItem: {0}, ScheduledUtc: {1:yyyy-MM-dd HH.mm.ss} Please verify if it is necessary sending your push again.", context.Task.ContentItem, context.Task.ScheduledUtc);
            }
        }
    }
}