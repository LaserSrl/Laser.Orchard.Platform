using Laser.Orchard.GDPR.Extensions;
using Laser.Orchard.GDPR.Services;
using Laser.Orchard.GDPR.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Environment.Extensions;
using Orchard.Services;
using Orchard.Settings;
using Orchard.Tasks.Scheduling;
using System;
using System.Linq;

namespace Laser.Orchard.GDPR.Handlers {
    [OrchardFeature("Laser.Orchard.GDPR.Scheduling")]
    public class GDPRTaskHandler : IScheduledTaskHandler {

        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IClock _clock;
        private readonly IScheduledTaskManager _taskManager;
        private readonly IContentGDPRManager _contentGDPRManager;
        private readonly ISiteService _siteService;

        public GDPRTaskHandler(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IClock clock,
            IScheduledTaskManager taskManager,
            IContentGDPRManager contentGDPRManager,
            ISiteService siteService) {

            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _clock = clock;
            _taskManager = taskManager;
            _contentGDPRManager = contentGDPRManager;
            _siteService = siteService;
        }

        public void Process(ScheduledTaskContext context) {
            // We have a task for each type that requires scheduled processing. We could have done 
            // this differently, by having a task for each ContentItem. The processing for each of
            // those would have been lighter. However, changes in the configuration for this task
            // would have been much harder to handle.

            string taskTypeStr = context.Task.TaskType;
            if (taskTypeStr.IndexOf(Constants.ScheduledTaskName) == 0) {
                // by splitting the string we should get a type name. To be safe, we do ElementAtOrDefault
                var typeName = taskTypeStr
                    .Split(new string[] { "_" }, 2, StringSplitOptions.RemoveEmptyEntries)
                    .ElementAtOrDefault(1);
                if (!string.IsNullOrWhiteSpace(typeName)) {
                    // get the settings for the type
                    var settings = _contentDefinitionManager.GetTypeDefinition(typeName)
                        .Parts
                        .FirstOrDefault(ctpd => ctpd.PartDefinition.Name == "GDPRPart")
                        ?.Settings
                        ?.GetModel<GDPRPartTypeSchedulingSettings>()
                        ?? new GDPRPartTypeSchedulingSettings();

                    if (settings.ScheduleAnonymization) {
                        Process(
                            typeName, 
                            settings.EventForAnonymization, 
                            settings.AnonymizationDaysToWait, 
                            _contentGDPRManager.Anonymize,
                            context.Task.ContentItem == null); // this being null means the task was not created here
                    }

                    if (settings.ScheduleErasure) {
                        Process(
                            typeName, 
                            settings.EventForErasure, 
                            settings.ErasureDaysToWait, 
                            _contentGDPRManager.Erase,
                            context.Task.ContentItem == null);
                    }
                    
                    if (settings.ScheduleAnonymization || settings.ScheduleErasure) {
                        // reschedule
                        _taskManager.CreateTask(
                            taskTypeStr, // "same" task
                            _clock.UtcNow + TimeSpan.FromDays(1), // one day from now
                            _siteService.GetSiteSettings().ContentItem // pass a ContentItem.
                            );
                        // passing a ContentItem there to the new task allows us to discriminate, when we are processing it,
                        // over whether the task being processed was created here or in the other service. This allows us to
                        // better filter over things in the query.
                    }
                }
            }
        }

        private void Process(
            string typeName, 
            EventForScheduling eventToCheck,
            int days, 
            Action<ContentItem> gdprProcess,
            bool firstTimeTask) {
            
            // get the contentItems to process
            var commonPartsQuery = _contentManager.Query()
                .ForType(typeName)
                .ForVersion(VersionOptions.AllVersions) // even deleted ContentItems
                .ForPart<CommonPart>();
            
            // we don't want to be pulling every ContentItem ever of this type. So, if we have processed things already
            // we pose a limit to the earliest date we will allow.
            if (!firstTimeTask) {
                commonPartsQuery = EarliestDateQuery(commonPartsQuery, eventToCheck, days);
            }
            commonPartsQuery = LatestDateQuery(commonPartsQuery, eventToCheck, days);

            var commonParts = commonPartsQuery
                .List() // becomes an IEnumerable
                ;
            var contentItems = commonParts
                .Select(cp => cp.ContentItem) // get the item
                .GroupBy(ci => ci.Id) // this line and the next basically do a Distinct() on the id
                .Select(group => group.First());

            foreach (var item in contentItems) {
                // After we process the item, we should revert the date of its last update.
                // If we don't, we will end up processing this item again in the future, because
                // its latest update will have moved forward to the date of this process.
                var oldDate = item.As<CommonPart>().ModifiedUtc;
                gdprProcess(item);
                item.As<CommonPart>().ModifiedUtc = oldDate;
            }
        }

        private IContentQuery<CommonPart> EarliestDateQuery(
            IContentQuery<CommonPart> query, EventForScheduling eventToCheck, int days) {

            var earliestDate = _clock.UtcNow - TimeSpan.FromDays(days + 2);
            switch (eventToCheck) {
                case EventForScheduling.Creation:
                    return query.Where<CommonPartRecord>(cpr => cpr.CreatedUtc > earliestDate);
                case EventForScheduling.LatestUpdate:
                default:
                    return query.Where<CommonPartRecord>(cpr => cpr.ModifiedUtc > earliestDate);
            }
        }

        private IContentQuery<CommonPart> LatestDateQuery(
            IContentQuery<CommonPart> query, EventForScheduling eventToCheck, int days) {

            var latestDate = _clock.UtcNow - TimeSpan.FromDays(days);
            switch (eventToCheck) {
                case EventForScheduling.Creation:
                    return query.Where<CommonPartRecord>(cpr => cpr.CreatedUtc < latestDate);
                case EventForScheduling.LatestUpdate:
                default:
                    return query.Where<CommonPartRecord>(cpr => cpr.ModifiedUtc < latestDate);
            }
        }
        
    }
}