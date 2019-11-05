using Laser.Orchard.Mobile.Models;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Mobile.ViewModels {
    public class PushIndex {
        public IList<PushNotificationRecord> PushRecords { get; set; }
        public dynamic Pager { get; set; }
        public PushSearch Search { get; set; }
        public List<string> MachineNames { get; set; }
        public PushIndex() {
        }
        public PushIndex(IEnumerable<PushNotificationRecord> pushrecords, PushSearch search, dynamic pager) {
            PushRecords = pushrecords.ToArray();
            Search = search;
            Pager = pager;        
        }
    }
}