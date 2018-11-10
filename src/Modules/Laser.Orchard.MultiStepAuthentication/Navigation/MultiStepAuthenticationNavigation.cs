using Laser.Orchard.MultiStepAuthentication.Controllers;
using Laser.Orchard.MultiStepAuthentication.Permissions;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MultiStepAuthentication.Navigation {
    [OrchardFeature("Laser.Orchard.MultiStepAuthentication")]
    public class MultiStepAuthenticationNavigation : INavigationProvider {

        private readonly IEnumerable<IMultiStepAdminController> _settingsControllers;

        public MultiStepAuthenticationNavigation(
            IEnumerable<IMultiStepAdminController> settingsControllers) {

            _settingsControllers = settingsControllers;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            if (_settingsControllers.Any()) {
                builder.Add(T("Settings"), menu => menu
                    .Add(T("MultiStep Authentication"), "10.0", submenu => {
                        submenu
                            .Action(_settingsControllers.First().ActionName, 
                                _settingsControllers.First().ControllerName, 
                                new { area = _settingsControllers.First().AreaName })
                            .Permission(MultiStepAuthenticationPermissions.ConfigureAuthentication);
                        foreach (var controller in _settingsControllers) {
                            submenu.Add(controller.Caption,
                                "10",
                                item => item
                                    .Action(controller.ActionName, 
                                        controller.ControllerName, 
                                        new { area = controller.AreaName })
                                    .Permission(MultiStepAuthenticationPermissions.ConfigureAuthentication)
                                    .LocalNav());
                        }
                    }));
            }
        }
    }
}