using Orchard;
using Orchard.ContentManagement;
using Orchard.UI.Navigation;
using System.Collections.Generic;

namespace Laser.Orchard.Cache.Services {
    public interface INavigationCacheService : IDependency {
        string GetMenuCacheKey(int menuContentItemId);
        IEnumerable<MenuItem> GetCachedMenuItems(IContent menu, string cacheKey, bool isDeepClone = true);
        IEnumerable<MenuItem> TryGetRouteValues(IEnumerable<MenuItem> menuItems);
    }
}
