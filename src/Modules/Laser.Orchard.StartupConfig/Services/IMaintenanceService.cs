using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IMaintenanceService: IDependency {
        List<MaintenanceVM> Get();
        List<MaintenanceVM> ListAll();
        List<string> GetAllTenantName();
    }
}