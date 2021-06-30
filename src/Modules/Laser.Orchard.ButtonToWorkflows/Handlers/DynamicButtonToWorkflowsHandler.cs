using System;
using System.Collections.Generic;
using Laser.Orchard.ButtonToWorkflows.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.UI.Notify;
using Orchard.Workflows.Services;

namespace Laser.Orchard.ButtonToWorkflows.Handlers {

    [OrchardFeature("Laser.Orchard.ButtonToWorkflows")]
    public class DynamicButtonToWorkflowsSettingsHandler : ContentHandler {

        public Localizer T { get; set; }

        public DynamicButtonToWorkflowsSettingsHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<DynamicButtonToWorkflowsSettingsPart>("Site"));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;

            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Buttons")));
        }
    }

    [OrchardFeature("Laser.Orchard.ButtonToWorkflows")]
    public class DynamicButtonToWorkflowsPartHandler : ContentHandler {

        private readonly INotifier _notifier;
        private readonly IScheduledTaskManager _scheduledTaskManager;
        private readonly IWorkflowManager _workflowManager;

        public Localizer T { get; set; }

        public DynamicButtonToWorkflowsPartHandler(
            INotifier notifier, 
            IScheduledTaskManager scheduledTaskManager, 
            IWorkflowManager workflowManager) {

            _notifier = notifier;
            _scheduledTaskManager = scheduledTaskManager;
            T = NullLocalizer.Instance;
            _workflowManager = workflowManager;

            OnUpdated<DynamicButtonToWorkflowsPart>((context, part) => {
                try {
                    if (!string.IsNullOrWhiteSpace(part.ButtonName)) {
                        var content = context.ContentItem;

                        if (part.ActionAsync) {
                            // the task will need to use part.ButtonName to invoke the correct
                            // process. We generate a task that contains that in its type. Then
                            // we will parse that out when processing the task.
                            _scheduledTaskManager
                                .CreateTask(
                                    DynamicButtonToWorflowsScheduledTaskHandler.TaskType(part.ButtonName), 
                                    DateTime.UtcNow.AddMinutes(1), 
                                    part.ContentItem);

                            if (!string.IsNullOrEmpty(part.MessageToWrite))
                                _notifier.Add(NotifyType.Information, T(part.MessageToWrite));
                        } else {
                            _workflowManager
                                .TriggerEvent("DynamicButtonEvent", 
                                    content, 
                                    () => new Dictionary<string, object> {
                                        { "ButtonName", part.ButtonName },
                                        { "Content", content } });

                            if (!string.IsNullOrEmpty(part.MessageToWrite))
                                _notifier.Add(NotifyType.Information, T(part.MessageToWrite));
                            
                        }
                        part.ButtonName = "";
                        part.MessageToWrite = "";
                    }
                } catch (Exception ex) {
                    Logger.Error(ex, "Error in DynamicButtonToWorkflowsPartHandler. ContentItem: {0}", context.ContentItem);

                    part.ButtonName = "";
                    part.MessageToWrite = "";
                }
            });
        }
    }
}