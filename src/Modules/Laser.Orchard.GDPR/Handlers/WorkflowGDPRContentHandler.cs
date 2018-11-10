using Orchard.Environment.Extensions;
using Orchard.Workflows.Services;
using System.Collections.Generic;

namespace Laser.Orchard.GDPR.Handlers {
    [OrchardFeature("Laser.Orchard.GDPR.Workflows")]
    public class WorkflowGDPRContentHandler : ContentGDPRHandler {

        private readonly IWorkflowManager _workflowManager;

        public WorkflowGDPRContentHandler(
            IWorkflowManager workflowManager) {

            _workflowManager = workflowManager;
            // We trigger the activities for the workflows. On top of that, we pass along
            // the GDPRContentContext object, so that it can be seen and used in the
            // workflows processing this stuff.
        }

        protected override void Anonymized(AnonymizeContentContext context) {
            // this check returns true if the item has a GDPRPart. It should not be required
            // but it's there for safety
            if (context.ShouldProcess(context.GDPRPart)) {
                _workflowManager.TriggerEvent("ContentAnonymized", context.ContentItem,
                    () => new Dictionary<string, object> { { "Content", context.ContentItem},
                        { "GDPRContentContext", context } });
            }
        }

        protected override void Erased(EraseContentContext context) {
            // this check returns true if the item has a GDPRPart. It should not be required
            // but it's there for safety
            if (context.ShouldProcess(context.GDPRPart)) {
                _workflowManager.TriggerEvent("ContentErased", context.ContentItem,
                    () => new Dictionary<string, object> { { "Content", context.ContentItem},
                        { "GDPRContentContext", context } });
            }
        }
    }
}