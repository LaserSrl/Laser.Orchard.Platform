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
        private const string TaskType = "Laser.Orchard.DynamicButtonToWorkflows.Task";

        public ILogger Logger { get; set; }

        public DynamicButtonToWorflowsScheduledTaskHandler(IWorkflowManager workflowManager) {
            _workflowManager = workflowManager;
            Logger = NullLogger.Instance;
        }

        public void Process(ScheduledTaskContext context) {
            try {
                if (context.Task.TaskType != TaskType) return;

                _workflowManager.TriggerEvent("DynamicButtonEvent", context.Task.ContentItem, () => 
                    new Dictionary<string, object> { { "ButtonName", context.Task.ContentItem.As<DynamicButtonToWorkflowsPart>().ButtonName }, { "Content", context.Task.ContentItem } });
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error in DynamicButtonToWorflowsScheduledTaskHandler. ContentItem: {0}, ScheduledUtc: {1:yyyy-MM-dd HH.mm.ss}", context.Task.ContentItem, context.Task.ScheduledUtc);
            }
            finally {
                if (context.Task.ContentItem.Has<DynamicButtonToWorkflowsPart>()) {
                    context.Task.ContentItem.As<DynamicButtonToWorkflowsPart>().ButtonName = "";
                    context.Task.ContentItem.As<DynamicButtonToWorkflowsPart>().MessageToWrite = "";
                    context.Task.ContentItem.As<DynamicButtonToWorkflowsPart>().ActionAsync = false;
                }
            }
        }
    }
}