using Laser.Orchard.GDPR.Extensions;
using Laser.Orchard.GDPR.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.Services;
#if DEBUG
using Orchard.Settings;
#endif
using Orchard.Tasks.Scheduling;
using System.Linq;

namespace Laser.Orchard.GDPR.Services {
    [OrchardFeature("Laser.Orchard.GDPR.Scheduling")]
    public class DefaultGDPRScheduleManager : IGDPRScheduleManager {

        private readonly IScheduledTaskManager _taskManager;
        private readonly IClock _clock;
        private readonly IContentDefinitionManager _contentDefinitionManager;
#if DEBUG
        // If we are debugging, a new call to UpdateSchedule where a task is scheduled already will
        // cause a reschedule that looks like the one performed at the end of the task's execution.
        // This way we can simulate the n-th task executions without having to wait for the required
        // delay.
        private readonly ISiteService _siteService;
#endif

        public DefaultGDPRScheduleManager(
            IScheduledTaskManager taskManager,
            IClock clock,
            IContentDefinitionManager contentDefinitionManager
#if DEBUG
            ,ISiteService siteService
#endif
            ) {

            _taskManager = taskManager;
            _clock = clock;
            _contentDefinitionManager = contentDefinitionManager;
#if DEBUG
            _siteService = siteService;
#endif
        }

        public void UpdateSchedule(string typeName, GDPRPartTypeSchedulingSettings settings) {

            // compute the name of the scheduled task for this type
            var taskName = string.Format(Constants.ScheduledTaskFormat, typeName);
            // check whether this type has a scheduled task
            var hasTask = _taskManager.GetTasks(taskName).Any();
            // if the settings say that the type should be processed, schedule a task
            if (settings.ScheduleAnonymization || settings.ScheduleErasure) {
                if (!hasTask) {
                    // create the task
                    _taskManager.CreateTask(taskName, _clock.UtcNow, null);
                } else {
                    // We run the risk of never processing some ContentItem in some conditions related to
                    // changes that happened to the configuration.
                    var oldSettings = SettingsFromType(typeName);
                    // Conditions describing the fact that the configuration has changed:
                    // - I should anonymize, but this was not the case before
                    var taskChanged = settings.ScheduleAnonymization && !oldSettings.ScheduleAnonymization;
                    // - I should erase, but this was not the case before
                    taskChanged |= settings.ScheduleErasure && !oldSettings.ScheduleErasure;
                    // - I should anonymize, as was the case before, but the time frame now is smaller
                    taskChanged |= (settings.ScheduleAnonymization && oldSettings.ScheduleAnonymization)
                        && settings.AnonymizationDaysToWait < oldSettings.AnonymizationDaysToWait;
                    // - I should erase, as was the case before, but the time frame now is smaller
                    taskChanged |= (settings.ScheduleErasure && oldSettings.ScheduleErasure)
                        && settings.ErasureDaysToWait < oldSettings.ErasureDaysToWait;
                    // - I should anonymize, as was the case before, but the event to check for anonymization has changed
                    taskChanged |= (settings.ScheduleAnonymization && oldSettings.ScheduleAnonymization)
                        && settings.EventForAnonymization != oldSettings.EventForAnonymization;
                    // - I should erase, as was the case before, but event to check for erasure has changed
                    taskChanged |= (settings.ScheduleErasure && oldSettings.ScheduleErasure)
                        && settings.EventForErasure != oldSettings.EventForErasure;

                    // If the configuration has changed, recreate the task
                    if (taskChanged) {
                        _taskManager.DeleteTasks(null, st => st.TaskType == taskName);
                        _taskManager.CreateTask(taskName, _clock.UtcNow, null);
                    }
#if DEBUG
                    else {
                        // we are debugging, so we recreate the task, passing a ContentItem to simulate the n-th execution
                        _taskManager.DeleteTasks(null, st => st.TaskType == taskName);
                        _taskManager.CreateTask(taskName, _clock.UtcNow, _siteService.GetSiteSettings().ContentItem);
                    }
#endif
                }
            } else {
                // this type is set to have no processing done
                if (hasTask) {
                    // destroy the task
                    _taskManager.DeleteTasks(null, st => st.TaskType == taskName);
                }
            }
        }

        private GDPRPartTypeSchedulingSettings SettingsFromType(string typeName) {
            var ctDefinition = _contentDefinitionManager.GetTypeDefinition(typeName);
            return ctDefinition
                ?.Parts
                ?.FirstOrDefault(ctpd => ctpd.PartDefinition.Name == "GDPRPart")
                ?.Settings
                .GetModel<GDPRPartTypeSchedulingSettings>()
                ?? new GDPRPartTypeSchedulingSettings();
        }

    }
}