using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Projections {
    public class SortCriterionConfiguration {
        public string PartName { get; set; }
        public string FieldName { get; set; }
        public string PartRecordTypeName { get; set; }
        public string PropertyName { get; set; }
        public bool Ascending { get; set; }

        public bool IsForField() {
            return !string.IsNullOrWhiteSpace(PartName)
                && !string.IsNullOrWhiteSpace(FieldName);
        }

        public bool IsForPart() {
            return !string.IsNullOrWhiteSpace(PartRecordTypeName)
                && !string.IsNullOrWhiteSpace(PropertyName);
        }
    }
}