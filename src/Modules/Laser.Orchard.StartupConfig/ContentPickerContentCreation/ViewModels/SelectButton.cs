using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.ContentPickerContentCreation.ViewModels {
    [OrchardFeature("Laser.Orchard.StartupConfig.ContentPickerContentCreation")]

    public class SelectButton {
        public string Callback { get; set; }

       public int IdContent { get; set; }

       public string NameCPFiels { get; set; }

       public string TitleContent { get; set; }

       public bool Published { get; set; }

    }
}