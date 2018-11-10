using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Facebook.ViewModels {

    public class SearchIndexVM {
        public IList<ContentIndexVM> ContentsIndexVM { get; set; }
        public dynamic Pager { get; set; }
        public dynamic Option { get; set; }
        public SearchVM Search { get; set; }

        public SearchIndexVM(IEnumerable<ContentIndexVM> contents, SearchVM search, dynamic pager, dynamic optionParameter = null) {
            ContentsIndexVM = contents.ToArray();
            Search = search;
            Pager = pager;
            Option = optionParameter;
        }
    }
}