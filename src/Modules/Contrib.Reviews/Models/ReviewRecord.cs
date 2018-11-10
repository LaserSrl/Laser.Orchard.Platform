using System;

namespace Contrib.Reviews.Models {
    public class ReviewRecord {
        public virtual int Id { get; set; }
        public virtual int ContentItemRecordId { get; set; }
        public virtual int VoteRecordId { get; set; }
        public virtual string Comment { get; set; }
        public virtual DateTime CreatedUtc { get; set; }
    }
}