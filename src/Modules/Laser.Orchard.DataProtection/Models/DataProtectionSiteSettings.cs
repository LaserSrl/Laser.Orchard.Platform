using Orchard.ContentManagement;

namespace Laser.Orchard.DataProtection.Models {
    public class DataProtectionSiteSettings : ContentPart {
        public bool ApplyToFrontEnd {
            get { return this.Retrieve(x => x.ApplyToFrontEnd); }
            set { this.Store(x => x.ApplyToFrontEnd, value); }
        }
    }
}