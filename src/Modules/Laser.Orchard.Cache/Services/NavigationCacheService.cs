using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Roles.Models;
using Orchard.Security.Permissions;
using Orchard.UI.Navigation;


namespace Laser.Orchard.Cache.Services {
    [OrchardFeature("Laser.Orchard.NavigationCache")]
    public class NavigationCacheService : INavigationCacheService {


        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly INavigationManager _navigationManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;


        public NavigationCacheService(
          ICacheManager cacheManager,
          ISignals signals,
          INavigationManager navigationManager,
          IOrchardServices orchardServices,
          IWorkContextAccessor workContextAccessor,
          IRepository<UserRolesPartRecord> userRolesRepository) {

            _cacheManager = cacheManager;
            _signals = signals;
            _navigationManager = navigationManager;
            _orchardServices = orchardServices;
            _workContextAccessor = workContextAccessor;
            _userRolesRepository = userRolesRepository;
        }


        public string GetMenuCacheKey(int menuContentItemId) {
            var cacheKey = string.Format("MenuWidgetPartDriverCacheKey_{0}", menuContentItemId);
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (user != null) {
                // Adding in a dependency on Orchard.Roles as it will be much more efficient to cache the
                //  navigation against the user roles than each logged in user (each logged in user wouldn't scale well)
                cacheKey = String.Format("{0}_{1}", cacheKey, "authenticated");
                var currentUserRoleRecords = _userRolesRepository.Fetch(x => x.UserId == user.Id);
                cacheKey = currentUserRoleRecords
                  .OrderBy(urr => urr.Role.Name)
                  .Aggregate(cacheKey, (current, userRoleRecord) => String.Format("{0}_{1}", current, userRoleRecord.Role.Id));
            }
            else {
                cacheKey = String.Format("{0}_{1}", cacheKey, "anonymous");
            }
            return cacheKey;
        }


        public IEnumerable<MenuItem> GetCachedMenuItems(IContent menu, string cacheKey, bool isDeepClone = true) {

            var cachedMenuItems = _cacheManager.Get(
              cacheKey,
              ctx => {
                  ctx.Monitor(_signals.When("NavigationContentItems.Changed"));
                  return _navigationManager.BuildMenu(menu).ToList();
              });

            if (!isDeepClone) {
                return cachedMenuItems;
            }

            // make a new list (clone) so that we don't persist changes to this list back into the cached values
            var menuItems = Clone(cachedMenuItems).ToArray();
            return menuItems;
        }


        private IContent GetCachedMenuPartContent(int contentId, string cacheKey) {

            var cachedMenuPartContent = _cacheManager.Get(
              cacheKey,
              ctx => {
                  ctx.Monitor(_signals.When("NavigationContentItems.Changed"));
                  var ci = _orchardServices.ContentManager.Get(contentId);
                  if (ci.Is<MenuPart>()) {
                      return ci.As<MenuPart>();
                  }
                  else {
                      return ci.As<IContent>();
                  }
              });
            return cachedMenuPartContent;
        }


        private IEnumerable<MenuItem> Clone(IEnumerable<MenuItem> cachedMenuItems) {

            // sanity check
            if (cachedMenuItems == null) {
                return Enumerable.Empty<MenuItem>();
            }

            return cachedMenuItems
                .Select(original => new MenuItem {
                    Text = original.Text,
                    IdHint = original.IdHint,
                    Url = original.Url,
                    Href = original.Href,
                    Position = original.Position,
                    LinkToFirstChild = original.LinkToFirstChild,
                    LocalNav = original.LocalNav,
                    Culture = original.Culture,
                    Selected = original.Selected,
                    RouteValues = original.RouteValues,
                    Items = Clone(original.Items).ToArray(),
                    Permissions = Clone(original.Permissions),
                    Content = GetCachedMenuPartContent(
                            original.Content.Id,
                            $"MenuWidgetPartDriverContetCacheKey_{original.Content.Id}"),
                    Classes = original.Classes
                });
        }


        private static IEnumerable<Permission> Clone(IEnumerable<Permission> cachedMenuItemPermissions) {

            var clone = new List<Permission>();

            if (cachedMenuItemPermissions != null) {
                clone.AddRange(cachedMenuItemPermissions.Select(cachedMenuItemPermission => new Permission {
                    Name = cachedMenuItemPermission.Name,
                    Description = cachedMenuItemPermission.Description,
                    Category = cachedMenuItemPermission.Category,
                    ImpliedBy = Clone(cachedMenuItemPermission.ImpliedBy)
                }));
            }

            return clone;
        }


        public IEnumerable<MenuItem> TryGetRouteValues(IEnumerable<MenuItem> menuItems) {
            foreach (var menuItem in menuItems) {
                if (menuItem.RouteValues == null) {
                    if (!String.IsNullOrEmpty(menuItem.Href)) {
                        menuItem.RouteValues = TryGetRouteValuesFromHref(menuItem.Href);
                    }
                }
                menuItem.Items = TryGetRouteValues(menuItem.Items);
            }
            return menuItems;
        }


        private RouteValueDictionary TryGetRouteValuesFromHref(string href) {

            RouteValueDictionary result = null;

            // Since GetRouteData (which is what Alias overrides) only accepts httpcontext, get context with passed in url
            //  The only way I can see this working is by using the current url context and see if it equals the href, should fix breadcrumb
            var httpContext = _workContextAccessor.GetContext().HttpContext;

            if (httpContext.Request.Path == href) {
                var routeData = httpContext.Request.RequestContext.RouteData;
                if (routeData != null) {
                    result = new RouteValueDictionary(routeData.Values);
                }
            }

            return result;
        }


    }
}