using System;
using Orchard.ContentManagement.Records;

namespace Contrib.Voting.Models {
    public class VoteRecord {
        public virtual int Id { get; set; }
        public virtual DateTime? CreatedUtc { get; set; }
        public virtual ContentItemRecord ContentItemRecord { get; set; }
        public virtual string ContentType { get; set; }
        public virtual string Username { get; set; }


        /// <summary>
        /// The IP address of the host the vote was cast from
        /// </summary>
        public virtual string Hostname { get; set; }

        /// <summary>
        /// The actual vote
        /// </summary>
        public virtual double Value { get; set; }

        /// <summary>
        /// A text representing the axe on which the vote is applied (i.e Quality, Support, ...). 'null' is the default value for an 'overall' vote.
        /// </summary>
        public virtual string Dimension { get; set; }
    }
}
