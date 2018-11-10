using Laser.Orchard.OpenAuthentication.Events;
using Orchard.Workflows.Services;
using System.Collections.Generic;

namespace Laser.Orchard.OpenAuthentication.Handlers {
    public class OpenAuthUserEventWorkflowTriggersHandler : IOpenAuthUserEventHandler {

        private readonly IWorkflowManager _workflowManager;

        public OpenAuthUserEventWorkflowTriggersHandler(
            IWorkflowManager workflowManager) {

            _workflowManager = workflowManager;
        }

        public void Created(CreatedOpenAuthUserContext context) {
            _workflowManager.TriggerEvent("OpenAuthUserCreated",
                context.User.ContentItem,
                () => new Dictionary<string, object> {
                    { "User", context.User },
                    { "ProviderName", context.ProviderName },
                    { "ProviderUserId", context.ProviderUserId },
                    { "ExtraData", context.ExtraData }
                });
        }

        public void Creating(CreatingOpenAuthUserContext context) {
            _workflowManager.TriggerEvent("OpenAuthUserCreating",
                null,
                () => new Dictionary<string, object> {
                    { "UserName", context.UserName },
                    { "EmailAddress", context.EmailAddress },
                    { "ProviderName", context.ProviderName },
                    { "ProviderUserId", context.ProviderUserId },
                    { "ExtraData", context.ExtraData }
                });
        }

        public void ProviderRecordCreated(CreatedOpenAuthUserContext context) {
            _workflowManager.TriggerEvent("OpenAuthRecordCreated",
                context.User.ContentItem,
                () => new Dictionary<string, object> {
                    { "User", context.User },
                    { "ProviderName", context.ProviderName },
                    { "ProviderUserId", context.ProviderUserId },
                    { "ExtraData", context.ExtraData }
                });
        }

        public void ProviderRecordUpdated(CreatedOpenAuthUserContext context) {
            _workflowManager.TriggerEvent("OpenAuthRecordUpdated",
                context.User.ContentItem,
                () => new Dictionary<string, object> {
                    { "User", context.User },
                    { "ProviderName", context.ProviderName },
                    { "ProviderUserId", context.ProviderUserId },
                    { "ExtraData", context.ExtraData }
                });
        }
    }
}