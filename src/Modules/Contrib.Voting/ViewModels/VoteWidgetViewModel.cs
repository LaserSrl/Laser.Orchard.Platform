using System.Collections.Generic;
using Contrib.Voting.Models;

namespace Contrib.Voting.ViewModels {
    public class VoteWidgetViewModel {
        public VoteWidgetPart Part { get; set; }
        public IEnumerable<string> ContentTypeNames { get; set; }
        public IEnumerable<string> FunctionNames { get; set; }
    }
}