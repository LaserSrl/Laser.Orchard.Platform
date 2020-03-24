using Nwazet.Commerce.Models;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class AddedMeasuringPurchase {
        public virtual int Id { get; set; } // Primary key
        public virtual OrderPartRecord OrderPartRecord { get; set; }
        public virtual bool AddedScript { get; set; }
    }
}