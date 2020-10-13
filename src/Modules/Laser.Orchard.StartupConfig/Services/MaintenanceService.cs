using AutoMapper;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Services {

    [OrchardFeature("Laser.Orchard.StartupConfig.Maintenance")]
    public class MaintenanceService : IMaintenanceService {
        private readonly IOrchardServices _orchardServices;
        private readonly IShellSettingsManager _shellSettingsManager;

        public MaintenanceService(
            IOrchardServices orchardServices,
            IShellSettingsManager shellSettingsManager,
            IOrchardHost orchardHost
            ) {
            _orchardServices = orchardServices;
            _shellSettingsManager = shellSettingsManager;
        }

        public List<MaintenanceVM> Get() {
            var listofcontentitems = _orchardServices.ContentManager.Query<MaintenancePart>(VersionOptions.Published).List();
            List<MaintenanceVM> ListMaintenanceVM = new List<MaintenanceVM>();

            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<MaintenancePart, MaintenanceVM>();
            });
            IMapper _mapper = mapperConfiguration.CreateMapper();

            foreach (var y in listofcontentitems) {
                MaintenanceVM MaintenanceVM = new MaintenanceVM();
                _mapper.Map<MaintenancePart, MaintenanceVM>(y.As<MaintenancePart>(), MaintenanceVM);
                ListMaintenanceVM.Add(MaintenanceVM);
            }
            return ListMaintenanceVM;
        }
        public List<MaintenanceVM> ListAll() {
            var listofcontentitems = _orchardServices.ContentManager.Query<MaintenancePart>(VersionOptions.Latest).List();
            List<MaintenanceVM> ListMaintenanceVM = new List<MaintenanceVM>();

            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<MaintenancePart, MaintenanceVM>();
            });
            IMapper _mapper = mapperConfiguration.CreateMapper();

            foreach (var y in listofcontentitems) {
                MaintenanceVM MaintenanceVM = new MaintenanceVM();
                _mapper.Map<MaintenancePart, MaintenanceVM>(y.As<MaintenancePart>(), MaintenanceVM);
                MaintenanceVM.IDcontentitem = y.Id;
                MaintenanceVM.Published = y.HasPublished();
                ListMaintenanceVM.Add(MaintenanceVM);
            }
            return ListMaintenanceVM;
        }

        public List<string> GetAllTenantName() {
            return _shellSettingsManager.LoadSettings().Select(x => x.Name).ToList();
        }
    }
}