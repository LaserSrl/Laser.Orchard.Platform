using System;
using System.Collections.Generic;
using Laser.Orchard.ButtonToWorkflows.Models;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.Workflows.Services;

namespace Laser.Orchard.ButtonToWorkflows.Handlers {
    public class DynamicButtonToWorflowsScheduledTaskHandler : IScheduledTaskHandler {

        private readonly IWorkflowManager _workflowManager;
        public static string BaseTaskType = "Laser.Orchard.DynamicButtonToWorkflows.Task";
        public static string TaskType(string buttonName) {
            return string.Format("{0}_{1}", BaseTaskType, buttonName);
        }

        public ILogger Logger { get; set; }

        public DynamicButtonToWorflowsScheduledTaskHandler(IWorkflowManager workflowManager) {
            _workflowManager = workflowManager;
            Logger = NullLogger.Instance;
        }

        public void Process(ScheduledTaskContext context) {
            try {
                var btnName = ButtonName(context.Task.TaskType);
                if (btnName == null) {
                    return;
                }
                if (string.IsNullOrWhiteSpace(btnName)) {
                    btnName = context.Task.ContentItem.As<DynamicButtonToWorkflowsPart>().ButtonName;
                }
                if (string.IsNullOrWhiteSpace(btnName)) {
                    // failed to figure out a valid button name
                    return;
                }

                _workflowManager.TriggerEvent("DynamicButtonEvent", context.Task.ContentItem, () => 
                    new Dictionary<string, object> {
                        { "ButtonName", btnName },
                        { "Content", context.Task.ContentItem } });
            } catch (Exception ex) {
                Logger.Error(ex, 
                    "Error in DynamicButtonToWorflowsScheduledTaskHandler. ContentItem: {0}, ScheduledUtc: {1:yyyy-MM-dd HH.mm.ss}", 
                    context.Task.ContentItem, context.Task.ScheduledUtc);
            } finally {
                if (context.Task.ContentItem.Has<DynamicButtonToWorkflowsPart>()) {
                    context.Task.ContentItem.As<DynamicButtonToWorkflowsPart>().ButtonName = "";
                    context.Task.ContentItem.As<DynamicButtonToWorkflowsPart>().MessageToWrite = "";
                }
            }
        }

        private string ButtonName(string taskType) {
            if (!taskType.StartsWith(BaseTaskType)) {
                // the task has nothing to do with DynamicButtonToWorkflow
                return null;
            }
            if (taskType.Equals(BaseTaskType, StringComparison.OrdinalIgnoreCase)) {
                // The task doesn't carry the name of the button in its TaskType.
                // This used to be the normal case, but it would prevent correct
                // execution of different scheduled tasks from the same ContentItem.
                return string.Empty;
            }
            return taskType.Substring(BaseTaskType.Length + 1);
        }
    }
}