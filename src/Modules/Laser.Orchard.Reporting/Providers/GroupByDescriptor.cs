using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Laser.Orchard.Reporting.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Laser.Orchard.Reporting.Providers {
    public class GroupByDescriptor
    {
        private Collection<AggregateMethods> aggregateMethods = new Collection<AggregateMethods>();

        public Collection<AggregateMethods> AggregateMethods
        {
            get
            {
                return this.aggregateMethods;
            }
        }

        public string Category { get; set; }
        public string Type { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public Func<IHqlQuery, AggregateMethods, IEnumerable<AggregationResult>> Run { get; set; }
        public Func<object, LocalizedString> FieldValueLabel { get; set; }
        public Func<FilterContext, LocalizedString> Display { get; set; }
    }
}