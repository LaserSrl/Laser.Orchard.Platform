using Laser.Orchard.UserReactions.Services;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Handlers {
    public class SettingsScheduledTaskHandler : IScheduledTaskHandler {
        private readonly IUserReactionsService _reactionsService;
        private const string TaskType = "Laser.Orchard.UserReactionsSettings.Task";

        public ILogger Logger { get; set; }

        public SettingsScheduledTaskHandler(IUserReactionsService reactionsService) {
            _reactionsService = reactionsService;
            Logger = NullLogger.Instance;
        }

        public void Process(ScheduledTaskContext context) {
            try {
                if (context.Task.TaskType != TaskType) {
                    return;
                }
                // esegue l'allineamento di tutti i contenuti in un task schedulato
                _reactionsService.NormalizeAllSummaries();
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error in Laser.Orchard.UserReactionsSettings.Task. ScheduledUtc: {0:yyyy-MM-dd HH.mm.ss}.", context.Task.ScheduledUtc);
            }
        }
    }
}