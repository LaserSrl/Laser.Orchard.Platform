using Laser.Orchard.GDPR.Settings;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.GDPR.ViewModels {
    [OrchardFeature("Laser.Orchard.GDPR.ContentPickerFieldExtension")]
    public class ContentPickerFieldGDPRPartFieldSettingsViewModel {

        public ContentPickerFieldGDPRPartFieldSettingsViewModel() {
            Settings = new ContentPickerFieldGDPRPartFieldSettings();
        }

        public ContentPickerFieldGDPRPartFieldSettings Settings { get; set; }

        public bool AttemptToAnonymizeItems {
            get { return Settings.AttemptToAnonymizeItems; }
            set { Settings.AttemptToAnonymizeItems = value; }
        }
        
        public bool DetachGDPRItemsOnAnonymize {
            get { return Settings.DetachGDPRItemsOnAnonymize; }
            set { Settings.DetachGDPRItemsOnAnonymize = value; }
        }

        public bool DetachAllItemsOnAnonymize {
            get { return Settings.DetachAllItemsOnAnonymize; }
            set { Settings.DetachAllItemsOnAnonymize = value; }
        }

        public bool AttemptToEraseItems {
            get { return Settings.AttemptToEraseItems; }
            set { Settings.AttemptToEraseItems = value; }
        }

        public bool DetachGDPRItemsOnErase {
            get { return Settings.DetachGDPRItemsOnErase; }
            set { Settings.DetachGDPRItemsOnErase = value; }
        }

        public bool DetachAllItemsOnErase {
            get { return Settings.DetachAllItemsOnErase; }
            set { Settings.DetachAllItemsOnErase = value; }
        }
        
        public string AnonymizationDivId { get; set; }

        public string ErasureDivId { get; set; }
    }
}