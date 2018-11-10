using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.Maintenance")]
    public class MaintenancePart : ContentPart<MaintenancePartRecord> {

        public NotifyType MaintenanceNotifyType {
            get { return this.Retrieve(r => r.MaintenanceNotifyType); }
            set { this.Store(r => r.MaintenanceNotifyType, value); }
        }

        public string MaintenanceNotify {
            get { return this.Retrieve(r => r.MaintenanceNotify); }
            set { this.Store(r => r.MaintenanceNotify, value); }
        }

        public string Selected_Tenant {
            get { return this.Retrieve(r => r.Selected_Tenant); }
            set { this.Store(r => r.Selected_Tenant, value); }
        }

    }
    [OrchardFeature("Laser.Orchard.StartupConfig.Maintenance")]
    public class MaintenancePartRecord : ContentPartVersionRecord {
        public virtual NotifyType MaintenanceNotifyType { get; set; }
        public virtual string MaintenanceNotify { get; set; }
        public virtual string Selected_Tenant { get; set; }
    }
}