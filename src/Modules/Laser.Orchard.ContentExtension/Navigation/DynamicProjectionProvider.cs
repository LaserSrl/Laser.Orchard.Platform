using Laser.Orchard.ContentExtension.Models;
using Laser.Orchard.ContentExtension.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;
using System.Collections.Generic;

namespace Laser.Orchard.ContentExtension.Navigation {
    [OrchardFeature("Laser.Orchard.ContentExtension.DynamicProjection")]
    public class DynamicProjectionProvider : INavigationProvider {
        private readonly IDynamicProjectionService _dynamicProjectionService;

        public DynamicProjectionProvider(
            IDynamicProjectionService dynamicProjectionService) {
            
            _dynamicProjectionService = dynamicProjectionService;
        }

        public string MenuName { get { return "admin"; } }

        private IEnumerable<DynamicProjectionPart> _allParts;
        public void GetNavigation(NavigationBuilder builder) {
            // GetNavigation is invoked (usually) twice, so it makes sense to memorize the
            // results of the query. (that's, twice per attempt to build a menu)
            if (_allParts == null) {
                _allParts = _dynamicProjectionService.GetPartsForMenu();
            }
            foreach (var part in _allParts) {
                if (part != null) {
                    builder.Add(
                        new LocalizedString(part.AdminMenuText),
                        part.AdminMenuPosition,
                        item => item.Action(
                            "List", 
                            "DynamicProjectionDisplay", 
                            new { area = "Laser.Orchard.ContentExtension", contentid = part.Id })
                            .Permission(Permissions.DynamicProjectionPermission.CreateDynamicPermission(part))
                            .AddImageSet(part.Icon));
                }
            }
        }
    }
}