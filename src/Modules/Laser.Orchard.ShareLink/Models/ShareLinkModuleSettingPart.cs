using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.ShareLink.Models {

    public class ShareLinkModuleSettingPart : ContentPart<ShareLinkModuleSettingPartRecord> {
        public string SharedBody {
            get { return this.Retrieve(r => r.SharedBody); }
            set { this.Store(r => r.SharedBody, value); }
        }

        public string SharedLink {
            get { return this.Retrieve(r => r.SharedLink); }
            set { this.Store(r => r.SharedLink, value); }
        }

        public string SharedText {
            get { return this.Retrieve(r => r.SharedText); }
            set { this.Store(r => r.SharedText, value); }
        }

        public string SharedImage {
            get { return this.Retrieve(r => r.SharedImage); }
            set { this.Store(r => r.SharedImage, value); }
        }

        public string Fb_App {
            get { return this.Retrieve(r => r.Fb_App); }
            set { this.Store(r => r.Fb_App, value); }
        }
    }

    public class ShareLinkModuleSettingPartRecord : ContentPartRecord {
        public virtual string SharedBody { get; set; }
        public virtual string SharedLink { get; set; }
        public virtual string SharedText { get; set; }
        public virtual string SharedImage { get; set; }
        public virtual string Fb_App { get; set; }
    }
}