using Orchard.ContentManagement;

namespace Laser.Orchard.ButtonToWorkflows.Models {
    public class DynamicButtonToWorkflowsPart : ContentPart {

        public string ButtonName
        {
            get { return this.Retrieve(r => r.ButtonName); }
            set { this.Store(r => r.ButtonName, value); }
        }

        public string MessageToWrite
        {
            get { return this.Retrieve(r => r.MessageToWrite); }
            set { this.Store(r => r.MessageToWrite, value); }
        }

        public bool ActionAsync
        {
            get { return this.Retrieve(r => r.ActionAsync); }
            set { this.Store(r => r.ActionAsync, value); }
        }
    }
}