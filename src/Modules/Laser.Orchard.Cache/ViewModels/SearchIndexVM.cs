using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.Cache.Models;
using Orchard.ContentManagement;

namespace Laser.Orchard.Cache.ViewModels {

    public class SearchIndexVM {
        public IList<CacheUrlRecord> ContentsIndexVM { get; set; }
        public dynamic Pager { get; set; }
        public CacheUrlSetting Option { get; set; }
        public SearchVM Search { get; set; }
        public string ImportErrors { get; set; }
        //  public bool Admn { get; set; }
        //  public int OrderPayedCount { get; set; }
        //      public Enums.OrderStatus stati { get; set; }

        //public OrderIndexVM() {
        //    //Search = new OrderSearchVM();
        //    //Search.ShowAll = false;
        //}

        public SearchIndexVM(IEnumerable<CacheUrlRecord> contents, SearchVM search, dynamic pager, CacheUrlSetting optionParameter = null) {
            ContentsIndexVM = contents.ToArray();
            Search = search;
            Pager = pager;
            Option = optionParameter;
        }
    }
}