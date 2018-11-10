using System;

namespace Laser.Orchard.Mobile.Settings {
    public class PushMobilePartSettingVM {
        public PushMobilePartSettingVM() {
            HideRelated = false;
        }

        public Boolean HideRelated { get; set; }
        public Boolean AcceptZeroRelated { get; set; }
        public string QueryDevice { get; set; }
    }
}