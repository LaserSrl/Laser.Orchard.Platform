using System.Collections.Generic;

namespace Laser.Orchard.ShareLink.ViewModels {

    public class ShareLinkVM {
        public string SharedLink { get; set; }
        public string SharedBody { get; set; }
        public string SharedText { get; set; }
        public string SharedImage { get; set; }
        public bool ShowSharedLink { get; set; }
        public bool ShowSharedBody { get; set; }
        public bool ShowSharedText { get; set; }
        public bool ShowSharedImage { get; set; }

        public ShareLinkVM() {
            ShowSharedLink = true;
            ShowSharedText = true;
            ShowSharedBody = true;
            ShowSharedImage = true;
        }

        public List<OptionList> ListOption { get; set; }
    }
}