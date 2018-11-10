using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Contrib.Voting.Models {
    public class VoteWidgetPart : ContentPart<VoteWidgetPartRecord> {
        [Required]
        public string FunctionName {
            get { return Record.FunctionName; }
            set { Record.FunctionName = value; }
        }

        public string ContentType {
            get { return Record.ContentType; }
            set { Record.ContentType = value; }
        }

        [Range(1, int.MaxValue)]
        public int Count {
            get { return Record.Count; }
            set { Record.Count = value; }
        }

        public bool Ascending {
            get { return Record.Ascending;  }
            set { Record.Ascending = value;  }
        }

        public string Dimension {
            get { return Record.Dimension; }
            set { Record.Dimension = value; }
        }
    }
}