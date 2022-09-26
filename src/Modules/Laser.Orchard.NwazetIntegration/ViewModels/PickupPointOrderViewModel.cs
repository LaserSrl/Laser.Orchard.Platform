using Laser.Orchard.NwazetIntegration.Models;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointOrderViewModel {
        public PickupPointOrderPart Part { get; set; }
        public bool AddressHasChanged { get; set; }
    }
}