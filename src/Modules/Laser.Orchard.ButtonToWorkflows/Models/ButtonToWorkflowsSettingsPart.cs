using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ButtonToWorkflows.Models {
    public class ButtonToWorkflowsSettingsPart : ContentPart<ButtonToWorkflowsSettingsPartRecord> {
        public string ButtonsText {
            get { return this.Retrieve(r => r.ButtonsText); }
            set { this.Store(r => r.ButtonsText, value); }
        }
        public string ButtonsAction {
            get { return this.Retrieve(r => r.ButtonsAction); }
            set { this.Store(r => r.ButtonsAction, value); }
        }
        public string ButtonsDescription {
            get { return this.Retrieve(r => r.ButtonsDescription); }
            set { this.Store(r => r.ButtonsDescription, value); }
        }
        public string ButtonsMessage {
            get { return this.Retrieve(r => r.ButtonsMessage); }
            set { this.Store(r => r.ButtonsMessage, value); }
        }
        public string ButtonsAsync
        {
            get { string val = this.Retrieve(r => r.ButtonsAsync);
            if (string.IsNullOrEmpty(val)) {
                return "false£false£false£false";
            }
            else
                return val;
            }
            set { this.Store(r => r.ButtonsAsync, value); }
        }
    }
    public class ButtonToWorkflowsSettingsPartRecord : ContentPartRecord {
        public virtual string ButtonsText { get; set; }
        public virtual string ButtonsAction { get; set; }
        public virtual string ButtonsDescription { get; set; }
        public virtual string ButtonsMessage { get; set; }
        public virtual string ButtonsAsync { get; set; }
    }
}

