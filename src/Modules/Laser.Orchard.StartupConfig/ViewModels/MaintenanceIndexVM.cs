using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ViewModels {
    public class MaintenanceIndexVM {
        public IList<dynamic> Maintenance { get; set; }
        public dynamic Pager { get; set; }
  //      public CustomersSearchVM Search { get; set; }

        public MaintenanceIndexVM() {
          //  Search = new CustomersSearchVM();
        }

        public MaintenanceIndexVM(IEnumerable<dynamic> maintenance, dynamic pager) {
            Maintenance = maintenance.ToArray();
           
            Pager = pager;
        }
    }
}