using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.NwazetIntegration.Activities;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Workflows.Services;

namespace Laser.Orchard.NwazetIntegration.Services.CheckoutConditions {
    public class WorkflowCheckoutCondition
        : ICheckoutCondition {

        private readonly IWorkflowManager _workflowManager;
        private readonly INotifier _notifier;

        public WorkflowCheckoutCondition(
            IWorkflowManager workflowManager,
            INotifier notifier) {

            _workflowManager = workflowManager;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public int Priority => 1;

        public bool UserMayCheckout(IUser user, out ActionResult redirect) {

            var context = CreateContext(user);
            InvokeValidation(context);

            redirect = context.Redirect;
            return context.UserMayCheckout;
        }

        public bool UserMayCheckout(IUser user) {

            var context = CreateContext(user);
            InvokeValidation(context);

            if (!context.UserMayCheckout) {
                if (context.Message != null) {
                    _notifier.Warning(context.Message);
                }
            }

            return context.UserMayCheckout;
        }

        private CheckoutConditionValidationContext CreateContext(IUser user) {
            // Activities manipulating this context should get further
            // services they may need themselves, to avoid creating unnecessary
            // dependencies.
            var context = new CheckoutConditionValidationContext {
                User = user
            };
            return context;
        }

        private void InvokeValidation(CheckoutConditionValidationContext context) {

            _workflowManager.TriggerEvent(
                ValidateCheckoutActivity.EventName,
                null,
                () => new Dictionary<string, object> {
                    {"Context", context}
                });
        }
    }
}