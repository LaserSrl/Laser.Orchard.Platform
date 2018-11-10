using Orchard.Environment.Extensions;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.ViewModels {
    [OrchardFeature("Laser.Orchard.StartupConfig.Maintenance")]
    public class MaintenanceVM {
        public int IDcontentitem { get; set; }
        public bool Published { get; set; }
        public NotifyType MaintenanceNotifyType { get; set; }
        public string MaintenanceNotify { get; set; }
        public IEnumerable<SelectListItem> List_Tenant { get; set; }
       // public string Selected_Tenant { get; set; }
        public string[] Selected_TenantVM { get; set; } 
        public string CurrentTenant { get; set; } 
    }
}