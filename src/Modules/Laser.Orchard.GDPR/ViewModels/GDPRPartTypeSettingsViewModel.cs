using Laser.Orchard.GDPR.Settings;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.GDPR.ViewModels {
    [OrchardFeature("Laser.Orchard.GDPR.Scheduling")]
    public class GDPRPartTypeSettingsViewModel {

        public GDPRPartTypeSettingsViewModel(){
            Settings = new GDPRPartTypeSettings();
        }

        public GDPRPartTypeSettings Settings { get; set; }
        
        public bool IsProfileItemType {
            get { return Settings.IsProfileItemType; }
            set { Settings.IsProfileItemType = value; }
        }

        public bool DeleteItemsAfterErasure {
            get { return Settings.DeleteItemsAfterErasure; }
            set { Settings.DeleteItemsAfterErasure = value; }
        }
    }
}