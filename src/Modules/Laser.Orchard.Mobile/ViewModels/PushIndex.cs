using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Mobile.ViewModels {
    public class PushIndex {

        public IList<dynamic> PushRecords { get; set; }
        public dynamic Pager { get; set; }
        public PushSearch Search { get; set; }
        public List<string> MachineNames { get; set; }
        public string SelectedMachineName { get; set; }

        public PushIndex() {
            //Search = new OrderSearchVM();
            //Search.ShowAll = false;
        }

        public PushIndex(IEnumerable<dynamic> pushrecords, PushSearch search, dynamic pager) {
            PushRecords = pushrecords.ToArray();
            Search = search;
            Pager = pager;        
        }


    }
}