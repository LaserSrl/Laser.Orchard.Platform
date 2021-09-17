using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.NwazetIntegration.Models {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopProductPart : ContentPart<FacebookShopProductPartRecord> {
        public static string PartName = "FacebookShopProductPart";
        
        public bool SynchronizeFacebookShop {
            get { return this.Retrieve(x => x.SynchronizeFacebookShop); }
            set { this.Store(x => x.SynchronizeFacebookShop, value); }
        }
    }
}