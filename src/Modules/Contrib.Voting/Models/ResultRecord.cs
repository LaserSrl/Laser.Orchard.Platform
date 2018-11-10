using System;
using Orchard.ContentManagement.Records;

namespace Contrib.Voting.Models {
    public class ResultRecord {
        public virtual int Id { get; set; }
        public virtual DateTime? CreatedUtc { get; set; }
        public virtual ContentItemRecord ContentItemRecord { get; set; }
        public virtual string ContentType { get; set; }
        public virtual string Dimension { get; set; }

        public virtual double Value { get; set; }
        public virtual int Count { get; set; }
        public virtual string FunctionName { get; set; }
    }
}
