namespace Laser.Orchard.GDPR.ViewModels {
    public class GDPRPartAdminToolbarLinkViewModel {

        public GDPRPartAdminToolbarLinkViewModel() {
            LinkController = "GDPRAdmin";
            LinkArea = "Laser.Orchard.GDPR";
        }

        public string LinkText { get; set; }
        public string LinkAction { get; set; }
        public string LinkController { get; set; }
        public string LinkArea { get; set; }
        public int ItemId { get; set; }
    }
}