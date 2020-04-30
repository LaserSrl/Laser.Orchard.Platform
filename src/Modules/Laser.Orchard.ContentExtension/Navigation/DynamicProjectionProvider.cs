using Laser.Orchard.ContentExtension.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;


namespace Laser.Orchard.ContentExtension.Navigation {
    [OrchardFeature("Laser.Orchard.ContentExtension.DynamicProjection")]
    public class DynamicProjectionProvider : INavigationProvider {
        private readonly IDynamicProjectionService _dynamicProjectionService;

        public DynamicProjectionProvider(
            IDynamicProjectionService dynamicProjectionService) {
            
            _dynamicProjectionService = dynamicProjectionService;
        }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            var allParts = _dynamicProjectionService.GetPartsForMenu();
            foreach (var part in allParts) {
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