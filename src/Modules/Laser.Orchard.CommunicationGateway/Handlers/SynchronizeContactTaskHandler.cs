using Laser.Orchard.CommunicationGateway.Services;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;

namespace Laser.Orchard.CommunicationGateway.Handlers {
    public class SynchronizeContactTaskHandler : IScheduledTaskHandler {
        private readonly ICommunicationService _communicationService;
        public ILogger Logger { get; set; }
        public const string TaskType = "Laser.Orchard.CommunicationGateway.SynchronizeContacts.Task";

        public SynchronizeContactTaskHandler(ICommunicationService communicationService) {
            _communicationService = communicationService;
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType != TaskType) {
                return;
            }
            try {
                Logger.Error("SynchronizeContacts task started.");
                _communicationService.Synchronize();
                Logger.Error("SynchronizeContacts task ended.");
            }
            catch (Exception ex) {
                Logger.Error(ex, "SynchronizeContacts task error."+ex.Message);
            }
        }
    }
}