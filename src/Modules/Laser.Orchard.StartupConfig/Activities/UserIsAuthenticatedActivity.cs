using Orchard;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Activities {
    public class UserIsAuthenticatedActivity : Task {

        private readonly IWorkContextAccessor _workContextAccessor;

        public UserIsAuthenticatedActivity(
            IWorkContextAccessor workContextAccessor) {

            _workContextAccessor = workContextAccessor;

            T = NullLocalizer.Instance;
        }


        public Localizer T { get; set; }

        public override string Name =>
            "UserIsAuthenticated";

        public override LocalizedString Category =>
            T("User");

        public override LocalizedString Description =>
            T("Tells whether the current request has an authenticated user.");

        public override IEnumerable<LocalizedString> Execute(
            WorkflowContext workflowContext, ActivityContext activityContext) {
            if (_workContextAccessor.GetContext().CurrentUser != null) {
                yield return T("Authenticated");
            } else {
                yield return T("Anonymous");
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(
            WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] {
                T("Authenticated"),
                T("Anonymous") };
        }
    }
}