using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class GTMProductPart : ContentPart<GTMProductPartRecord> {
        /// <summary>
        /// In property definition Javascript ProductId must be Id
        /// </summary>
        [Required]
        public string ProductId {
            get { return this.Retrieve(r => r.ProductId); }
            set { this.Store(r => r.ProductId, value); }
        }
        [Required]
        public string Name {
            get { return this.Retrieve(r => r.Name); }
            set { this.Store(r => r.Name, value); }
        }
        public string Brand {
            get { return this.Retrieve(r => r.Brand); }
            set { this.Store(r => r.Brand, value); }
        }
        public string Category {
            get { return this.Retrieve(r => r.Category); }
            set { this.Store(r => r.Category, value); }
        }
        public string Variant {
            get { return this.Retrieve(r => r.Variant); }
            set { this.Store(r => r.Variant, value); }
        }
        public decimal Price {
            get { return this.Retrieve(r => r.Price); }
            set { this.Store(r => r.Price, value); }
        }
    }

}