using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.ButtonToWorkflows.Models;
using Laser.Orchard.ButtonToWorkflows.Services;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Security.Permissions;

namespace Laser.Orchard.ButtonToWorkflows.Security {
    public class DynamicButtonPermissions : IPermissionProvider {

        private readonly IDynamicButtonToWorkflowsService _dynamicButtonToWorkflowsService;

        public Localizer T { get; set; }

        public DynamicButtonPermissions(IDynamicButtonToWorkflowsService dynamicButtonToWorkflowsService) {
            _dynamicButtonToWorkflowsService = dynamicButtonToWorkflowsService;
            T = NullLocalizer.Instance;
        }

        public virtual Feature Feature { get; set; }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

        public IEnumerable<Permission> GetPermissions() {
            List<Permission> permissions = new List<Permission>();
            IList<DynamicButtonToWorkflowsRecord> dynamicButtons = _dynamicButtonToWorkflowsService.GetButtons();

            foreach (var button in dynamicButtons) {
                permissions.Add(_dynamicButtonToWorkflowsService.GetButtonPermission(button.ButtonName));
            }

            return permissions;
        }
    }
}