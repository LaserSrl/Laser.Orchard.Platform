using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.CacheEvictorPart")]
    public class CacheEvictorPartSettings {
        public string EvictItem { get; set; }
        public string IdentityEvictItem { get; set; }
    }
}