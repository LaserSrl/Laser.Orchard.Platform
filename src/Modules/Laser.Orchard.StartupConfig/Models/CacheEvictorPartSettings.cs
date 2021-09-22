using Orchard.Environment.Extensions;
using Orchard.Taxonomies.Models;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.CacheEvictorPart")]
    public class CacheEvictorPartSettings {
        public CacheEvictorPartSettings() {
            FilterTermsId = new List<int>();
        }
        public string EvictItem { get; set; }
        public string IdentityEvictItem { get; set; }
        public IEnumerable<TaxonomyPart> TaxonomiesPart { get; set; }
        public IEnumerable<int> FilterTermsId { get; set; }
        public string FilterTermsRecordId { get; set; }
        public string IdentityFilterTerms { get; set; }
        public bool IncludeChildren { get; set; }
    }
}