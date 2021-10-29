using Orchard.Environment.Extensions;
using Orchard.Projections.ViewModels;
using Orchard.Taxonomies.Models;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.CacheEvictorPart")]
    public class CacheEvictorPartSettings {
        public CacheEvictorPartSettings() {
            EvictItem = string.Empty;
            IdentityEvictItem = string.Empty;
            QueryRecordEntries = new List<QueryRecordEntry>();
            FilterQueryRecordsId = new List<string>();
            FilterQueryRecordId = string.Empty;
            IdentityFilterQueryRecord = string.Empty;
        }
        public string EvictItem { get; set; }
        public string IdentityEvictItem { get; set; }
        public bool EvictTerms { get; set; }
        public IEnumerable<QueryRecordEntry> QueryRecordEntries { get; set; }
        public IEnumerable<string> FilterQueryRecordsId { get; set; }
        public string FilterQueryRecordId { get; set; }
        public string IdentityFilterQueryRecord { get; set; }
    }

    public class QueryRecordEntry {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}