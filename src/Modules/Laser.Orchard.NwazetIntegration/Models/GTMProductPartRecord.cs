using Orchard.ContentManagement.Records;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class GTMProductPartRecord : ContentPartRecord {
        public virtual string ProductId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Brand { get; set; }
        public virtual string Category { get; set; }
        public virtual string Variant { get; set; }
        public virtual decimal Price { get; set; }
    }
}