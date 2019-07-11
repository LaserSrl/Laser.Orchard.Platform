using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.ContentPickerContentCreation.Settings {
    [OrchardFeature("Laser.Orchard.StartupConfig.ContentPickerContentCreation")]
    public class CPContentCreationSettings {

        public bool EnableContentCreation { get; set; }

    }
}