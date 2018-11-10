using Orchard.ContentManagement.Records;

namespace Contrib.Voting.Models {
    public class VoteWidgetPartRecord : ContentPartRecord {
        public virtual string FunctionName { get; set; }
        public virtual string ContentType { get; set; }
        public virtual string Dimension { get; set; }
        public virtual int Count { get; set; }
        public virtual bool Ascending { get; set; }
    }
}