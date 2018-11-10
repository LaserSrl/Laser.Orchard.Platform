using Laser.Orchard.ContentExtension.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;


namespace Laser.Orchard.ContentExtension.Navigation {
    [OrchardFeature("Laser.Orchard.ContentExtension.DynamicProjection")]
    public class DynamicProjectionProvider : INavigationProvider {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public DynamicProjectionProvider(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager) {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            var allParts = _contentManager.Query<DynamicProjectionPart, DynamicProjectionPartRecord>().Where(x => x.OnAdminMenu).List();
            foreach (var part in allParts) {
                if (part != null) {
                    var readpermission = new Permissions.DynamicProjectionPermission(_contentManager);
                    readpermission.GetPermissions();
                        builder.Add(new LocalizedString(part.AdminMenuText),
                                  part.AdminMenuPosition,
                                  item => item.Action("List", "DynamicProjectionDisplay", new { area = "Laser.Orchard.ContentExtension", contentid = part.Id })
                                 .Permission(Permissions.DynamicProjectionPermission.PermissionsList["DynamicProjectionPermission" + part.Id.ToString()])
                                 .AddImageSet(part.Icon));
                }
            }
        }
    }
}