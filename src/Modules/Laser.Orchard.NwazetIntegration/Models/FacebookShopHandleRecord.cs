using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.NwazetIntegration.Models {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopHandleRecord {
        public virtual int Id { get; set; }
        [StringLengthMax]
        public virtual string RequestJson { get; set; }
        public virtual string Handle { get; set; }
        public virtual bool Processed { get; set; }
    }
}