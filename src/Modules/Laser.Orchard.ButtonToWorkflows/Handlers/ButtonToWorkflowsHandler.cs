using Laser.Orchard.ButtonToWorkflows.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Workflows.Services;
using System.Collections.Generic;
using Orchard.Tasks.Scheduling;
using System;

namespace Laser.Orchard.ButtonToWorkflows.Handlers {

    [OrchardFeature("Laser.Orchard.ButtonToWorkflows")]
    public class SiteSettingsPartHandler : ContentHandler {
        public SiteSettingsPartHandler(IRepository<ButtonToWorkflowsSettingsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<ButtonToWorkflowsSettingsPart>("Site"));
            T = NullLocalizer.Instance;
            OnGetContentItemMetadata<ButtonToWorkflowsSettingsPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Buttons"))));
        }
        public Localizer T { get; set; }
    }

    [OrchardFeature("Laser.Orchard.ButtonToWorkflows")]
    public class ButtonToWorkflowsPartHandler : ContentHandler {
        public Localizer T { get; set; }
        private readonly IWorkflowManager _workflowManager;
        private readonly INotifier _notifier;
        private readonly IScheduledTaskManager _scheduledTaskManager;

        public ButtonToWorkflowsPartHandler(IWorkflowManager workflowManager, INotifier notifier, IScheduledTaskManager scheduledTaskManager) {
            _workflowManager = workflowManager;
            _notifier = notifier;
            _scheduledTaskManager = scheduledTaskManager;
            T = NullLocalizer.Instance;

            OnUpdated<ButtonToWorkflowsPart>((context, part) => {
                if (!string.IsNullOrEmpty(part.ActionToExecute)) {
                    var content = context.ContentItem;
                    if (part.ActionAsync) {
                      //  part.ButtonsDenied = true;
                        _scheduledTaskManager.CreateTask("Laser.Orchard.ButtonToWorkflows.Task", DateTime.UtcNow.AddMinutes(1), part.ContentItem);
                    }
                    else {
                        _workflowManager.TriggerEvent(part.ActionToExecute, content, () => new Dictionary<string, object> { { "Content", content } });
                        part.MessageToWrite = "";
                        part.ActionToExecute = "";
                        part.ActionAsync = false;
                    }
                    try {
                        if (!string.IsNullOrEmpty(part.MessageToWrite))
                            _notifier.Add(NotifyType.Information, T(part.MessageToWrite));
                    }
                    catch { }
            
                }

                //  if (context.ContentItem.As<CommonPart>() != null) {
                //    var currentUser = _orchardServices.WorkContext.CurrentUser;
                //if (currentUser != null) {
                //    ((dynamic)context.ContentItem.As<CommonPart>()).LastModifier.Value = currentUser.Id;
                //    if (((dynamic)context.ContentItem.As<CommonPart>()).Creator.Value == null)
                //        //  ((NumericField) CommonPart.Fields.Where(x=>x.Name=="Creator").FirstOrDefault()).Value = currentUser.Id;
                //        ((dynamic)context.ContentItem.As<CommonPart>()).Creator.Value = currentUser.Id;
                //}
                //   }

            });
        }
    }
}