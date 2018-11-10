using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.ButtonToWorkflows.Models;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.Workflows.Services;
using Orchard.ContentManagement;

namespace Laser.Orchard.ButtonToWorkflows.Handlers {
    public class ButtonToWorflowsScheduledTaskHandler : IScheduledTaskHandler {
        private readonly IWorkflowManager _workflowManager;
        private const string TaskType = "Laser.Orchard.ButtonToWorkflows.Task";
        public ILogger Logger { get; set; }
        public ButtonToWorflowsScheduledTaskHandler(IWorkflowManager workflowManager) {
            Logger = NullLogger.Instance;
            _workflowManager = workflowManager;
        }

        public void Process(ScheduledTaskContext context) {
            try {
                if (context.Task.TaskType != TaskType) {
                    return;
                }
                 _workflowManager.TriggerEvent(context.Task.ContentItem.As<ButtonToWorkflowsPart>().ActionToExecute, context.Task.ContentItem, () => new Dictionary<string, object> { { "Content", context.Task.ContentItem } });
                 context.Task.ContentItem.As<ButtonToWorkflowsPart>().MessageToWrite = "";
                 context.Task.ContentItem.As<ButtonToWorkflowsPart>().ActionToExecute = "";
                 context.Task.ContentItem.As<ButtonToWorkflowsPart>().ActionAsync = false;
                 context.Task.ContentItem.As<ButtonToWorkflowsPart>().ButtonsDenied = false;
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error in ButtonToWorflowsScheduledTaskHandler. ContentItem: {0}, ScheduledUtc: {1:yyyy-MM-dd HH.mm.ss}", context.Task.ContentItem, context.Task.ScheduledUtc);
            }
        }
    }
}