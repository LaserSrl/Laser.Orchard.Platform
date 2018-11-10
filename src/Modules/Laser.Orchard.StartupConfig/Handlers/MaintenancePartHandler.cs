using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Handlers {
     [OrchardFeature("Laser.Orchard.StartupConfig.Maintenance")]
    public class MaintenancePartHandler : ContentHandler {
         public MaintenancePartHandler(IRepository<MaintenancePartRecord> MaintenanceRepository) {
             Filters.Add(StorageFilter.For(MaintenanceRepository));
         }
    }
}