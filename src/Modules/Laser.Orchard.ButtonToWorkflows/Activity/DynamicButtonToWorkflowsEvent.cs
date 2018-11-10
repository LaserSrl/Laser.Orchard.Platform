using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.ButtonToWorkflows.Services;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Laser.Orchard.ButtonToWorkflows.Activity {
    public class DynamicButtonToWorkflowsEvent : Event {

        private readonly IDynamicButtonToWorkflowsService _dynamicButtonToWorkflowsService;
        public Localizer T { get; set; }

        public DynamicButtonToWorkflowsEvent(IDynamicButtonToWorkflowsService dynamicButtonToWorkflowsService) {
            _dynamicButtonToWorkflowsService = dynamicButtonToWorkflowsService;
            T = NullLocalizer.Instance;
        }

        public override bool CanStartWorkflow
        {
            get { return true; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done") };
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            try {
                // Recupero il bottone associato all'evento
                var buttonSelected = activityContext.GetState<string>("DynamicButton");

                // Verifico che all'evento sia effettivamente associato un bottone e di avere le informazioni sul bottone cliccato nel workflowContext
                if (!string.IsNullOrWhiteSpace(buttonSelected) && workflowContext.Tokens.ContainsKey("ButtonName")) {
                    // Recupero il bottone che è stato cliccato. Se esiste, ne ottengo il Guid e lo confronto con quello del bottone selezionato.
                    var clickedButtonName = workflowContext.Tokens["ButtonName"].ToString();

                    if (!string.IsNullOrWhiteSpace(clickedButtonName)) {
                        var clickedButtonIdentifier = _dynamicButtonToWorkflowsService.GetButtons().Where(w => w.ButtonName == clickedButtonName).Select(s => s.GlobalIdentifier).FirstOrDefault();
                        return clickedButtonIdentifier.ToString() == buttonSelected;
                    }
                }

                return false;
            }
            catch {
                return false;
            }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override LocalizedString Category
        {
            get { return T("Content Event"); }
        }

        public override string Name
        {
            get { return "DynamicButtonEvent"; }
        }

        public override string Form
        {
            get { return "_DynamicButtonSelectForm"; }
        }

        public override LocalizedString Description
        {
            get { return T("Dynamic button is clicked."); }
        }
    }
}