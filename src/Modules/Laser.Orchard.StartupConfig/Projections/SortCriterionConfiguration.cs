using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Projections {
    public class SortCriterionConfiguration {
        public SortCriterionConfiguration(){
            Children = new List<SortCriterionConfiguration>();
        }
        public string PartName { get; set; }
        public string FieldName { get; set; }
        public string PartRecordTypeName { get; set; }
        public string PropertyName { get; set; }
        public bool Ascending { get; set; }
        // ability to configure other existing providers
        public string SortCriterionProviderCategory { get; set; }
        public string SortCriterionProviderType { get; set; }
        public string SortCriterionProviderState { get; set; }

        public List<SortCriterionConfiguration> Children { get; set; }

        /// <summary>
        /// This string is not used in processing the sorting, but it's made available
        /// to avoid having to use dynamic types in case we want to display in frontend
        /// a selection among configured criteria.
        /// </summary>
        public string FilterLabel { get; set; }

        public bool IsForField() {
            return !string.IsNullOrWhiteSpace(PartName)
                && !string.IsNullOrWhiteSpace(FieldName);
        }

        public bool IsForPart() {
            return !string.IsNullOrWhiteSpace(PartRecordTypeName)
                && !string.IsNullOrWhiteSpace(PropertyName);
        }

        public bool IsForProvider() {
            return !string.IsNullOrWhiteSpace(SortCriterionProviderCategory)
                && !string.IsNullOrWhiteSpace(SortCriterionProviderType);
        }
    }
}