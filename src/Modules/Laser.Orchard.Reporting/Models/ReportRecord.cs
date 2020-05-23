using Orchard.Data.Conventions;
using Orchard.Projections.Models;

namespace Laser.Orchard.Reporting.Models {
    public class ReportRecord 
    {
        public virtual int Id { get; set; }
        public virtual string Title { get; set; }
        public virtual string Name { get; set; }

        public virtual string ColumnAliases { get; set; }

        [Aggregate]
        public virtual QueryPartRecord Query { get; set; }
        public virtual string State { get; set; }
        public virtual string GroupByCategory { get; set; }
        public virtual string GroupByType { get; set; }
        public virtual int AggregateMethod { get; set; }

    }
}