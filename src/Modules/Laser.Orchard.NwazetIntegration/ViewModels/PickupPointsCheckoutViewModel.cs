using Laser.Orchard.NwazetIntegration.Models;
using Newtonsoft.Json;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class PickupPointsCheckoutViewModel {
        public int SelectedPickupPointId { get; set; }
        [JsonIgnore]
        public PickupPointPart PickupPointPart { get; set; }
    }
}