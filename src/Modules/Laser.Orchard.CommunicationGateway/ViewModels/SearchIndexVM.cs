using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.CommunicationGateway.ViewModels {

    public class SearchIndexVM {
        public IList<ContentIndexVM> ContentsIndexVM { get; set; }
        public dynamic Pager { get; set; }
        public dynamic Option { get; set; }
        public SearchVM Search { get; set; }
        public string ImportErrors { get; set; }
        //  public bool Admn { get; set; }
        //  public int OrderPayedCount { get; set; }
        //      public Enums.OrderStatus stati { get; set; }

        //public OrderIndexVM() {
        //    //Search = new OrderSearchVM();
        //    //Search.ShowAll = false;
        //}

        public SearchIndexVM(IEnumerable<ContentIndexVM> contents, SearchVM search, dynamic pager, dynamic optionParameter = null) {
            ContentsIndexVM = contents.ToArray();
            Search = search;
            Pager = pager;
            Option = optionParameter;
        }
    }
}