using Orchard.Environment.Extensions;
using Orchard.Taxonomies.Models;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.CacheEvictorPart")]
    public class CacheEvictorPartSettings {
        public string EvictItem { get; set; }
        public string IdentityEvictItem { get; set; }
        public bool EvictTerms { get; set; }
    }
}