using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.ContentPickerContentCreation.ViewModels {
    [OrchardFeature("Laser.Orchard.StartupConfig.ContentPickerContentCreation")]

    public class ContentPickerCreationWindowVM {

        public int IdContent { get; set; }

        public string TitleContent { get; set; }

        public string TypeContent { get; set; }

        public bool Published { get; set; }

    }
}