using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
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
        public int Quantity {
            get { return this.Retrieve(r => r.Quantity); }
            set { this.Store(r => r.Quantity, value); }
        }
        public string Coupon {
            get { return this.Retrieve(r => r.Coupon); }
            set { this.Store(r => r.Coupon, value); }
        }
        public int Position {
            get { return this.Retrieve(r => r.Position); }
            set { this.Store(r => r.Position, value); }
        }
    }

    public class GTMProductPartRecord : ContentPartRecord {
        public virtual string ProductId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Brand { get; set; }
        public virtual string Category { get; set; }
        public virtual string Variant { get; set; }
        public virtual decimal Price { get; set; }
        public virtual int Quantity { get; set; }
        public virtual string Coupon { get; set; }
        public virtual int Position { get; set; }
    }
}