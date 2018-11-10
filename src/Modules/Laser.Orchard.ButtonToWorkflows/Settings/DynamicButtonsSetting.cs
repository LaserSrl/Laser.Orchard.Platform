using System;

namespace Laser.Orchard.ButtonToWorkflows.Settings {
    public class DynamicButtonsSetting {
        public string Buttons { get; set; }

        public string[] List
        {
            get { return String.IsNullOrEmpty(Buttons) ? new string[0] : Buttons.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); }
            set { Buttons = String.Join(",", value); }
        }
    }
}