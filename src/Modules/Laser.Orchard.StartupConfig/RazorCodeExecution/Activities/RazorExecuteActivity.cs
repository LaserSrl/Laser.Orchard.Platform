using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.RazorCodeExecution.Services;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Laser.Orchard.StartupConfig.RazorCodeExecution.Activities {

    [OrchardFeature("Laser.Orchard.StartupConfig.WorkflowExtensions")]
    public class RazorExecuteActivity : Task {
        private readonly IOrchardServices _services;
        private readonly IRazorExecuteService _razorExecuteService;

        public RazorExecuteActivity(IOrchardServices services, IRazorExecuteService razorExecuteService) {
            _services = services;
            _razorExecuteService = razorExecuteService;
        }



        public Localizer T { get; set; }

        /// <summary>
        /// Name of the Activity
        /// Note: in order to customize view in the workflow, the workflow engine fires a shape named Activity-{Name}.cshtml (e.g. Activity-RazorExecute.cshtml). 
        /// This approach permits to inflate possible outcomes at runtime. Showing at ~/Modules/Orchard.Scripting.CSharp/Views/Activity-Decision.cshtml.
        /// </summary>
        public override string Name {
            get { return "RazorExecute"; }
        }

        /// <summary>
        /// The name of the form defined in IFormProvider Implementation, context.Form("RazorExecuteForm", form);
        /// </summary>
        public override string Form {
            get { return "RazorExecuteForm"; }
        }

        public override LocalizedString Category {
            get { return T("Scripting"); }
        }
        public override LocalizedString Description {
            get { return T("Execute code within a razor view."); }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var result = _razorExecuteService.Execute(activityContext.GetState<string>("RazorExecuteActivity_RazorView"), workflowContext.Content, workflowContext.Tokens).Trim();
            if (result == null) {
                result = "Error";
            } else if (result == "") {
                result = "Empty";
            }
            yield return T(result);
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return GetOutcomes(activityContext).Select(outcome => T(outcome));
        }


        private IEnumerable<string> GetOutcomes(ActivityContext context) {

            var outcomes = context.GetState<string>("RazorExecuteActivity_Outcomes");

            if (String.IsNullOrEmpty(outcomes)) {
                return new List<string> {"Error","Empty"};
            }
            var defoutcomes = outcomes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            defoutcomes.AddRange(new List<string> { "Error", "Empty" });
            return defoutcomes;

        }

    }
}

