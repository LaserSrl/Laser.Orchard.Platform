using Laser.Orchard.ButtonToWorkflows.ViewModels.ValidationProviders;

namespace Laser.Orchard.ButtonToWorkflows.ViewModels {
    [ValidateDynamicButtonData]
    public class DynamicButtonToWorkflowsEdit {
        public int Id { get; set; }

        public string ButtonName { get; set; }

        public string ButtonText { get; set; }

        public string ButtonDescription { get; set; }

        public string ButtonMessage { get; set; }

        public bool ButtonAsync { get; set; }

        public string GlobalIdentifier { get; set; }

        public bool Delete { get; set; }
    }
}