using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using System.Web.Helpers;

//using Orchard.Scripting.CSharp.Services;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using Orchard;
using Laser.Orchard.StartupConfig.RazorCodeExecution.Services;
using Orchard.Mvc.Html;

namespace Laser.Orchard.RazorScripting.Activities {

    public class RazorDecisionActivity : Task {
        private readonly IRazorExecuteService _razorExecuteService;
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkContextAccessor _workContextAccessor;

        public RazorDecisionActivity(
            IOrchardServices orchardServices,
            IRazorExecuteService razorExecuteService,
            IWorkContextAccessor workContextAccessor) {
            _razorExecuteService = razorExecuteService;
            _orchardServices = orchardServices;
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name {
            get { return "RazorDecision"; }
        }

        public override LocalizedString Category {
            get { return T("Misc"); }
        }

        public override LocalizedString Description {
            get { return T("Evaluates an expression."); }
        }

        public override string Form {
            get { return "RazorActivityActionDecision"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return GetOutcomes(activityContext).Select(outcome => T.Encode(outcome));
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            string script = Json.Decode(activityContext.Record.State).RazorScript;
            //  var script = "@{" + activityContext.GetState<string>("RazorScript") + "}"; Orchard mi elimina il codice scritto nelle graffe
            string outcome = _razorExecuteService.ExecuteString(script, (dynamic)workflowContext.Content.ContentItem, null);
            outcome = (outcome ?? "").Replace("\r\n", "");
            yield return T.Encode(Convert.ToString(outcome));
        }

        private IEnumerable<string> GetOutcomes(ActivityContext context) {
            var outcomes = context.GetState<string>("Outcomes");

            if (String.IsNullOrEmpty(outcomes)) {
                return Enumerable.Empty<string>();
            }

            return outcomes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }
    }
}