using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.NwazetIntegration.Models {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopProductPartRecord : ContentPartRecord {
        public FacebookShopProductPartRecord() {
            SynchronizeFacebookShop = true;
        }
        public virtual bool SynchronizeFacebookShop { get; set; }
    }
}