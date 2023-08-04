using Laser.Orchard.Sharing.Settings;

namespace Laser.Orchard.Sharing.ViewModels {
    public class ShareBarViewModel {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Media { get; set; }
        public string Description { get; set; }
        public ShareBarTypePartSettings Settings { get; set; }
    }
}