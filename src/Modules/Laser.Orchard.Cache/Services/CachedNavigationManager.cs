using Laser.Orchard.Cache.Models;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Services;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Mvc;
using Orchard.Roles.Models;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.Cache.Services {
    [OrchardFeature("Laser.Orchard.NavigationCache")]
    public class CachedNavigationManager : NavigationManager, INavigationManager {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IContentManager _contentManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CachedNavigationManager(
            IEnumerable<INavigationProvider> navigationProviders,
            IEnumerable<IMenuProvider> menuProviders,
            IAuthorizationService authorizationService,
            IEnumerable<INavigationFilter> navigationFilters,
            UrlHelper urlHelper,
            IOrchardServices orchardServices,
            ShellSettings shellSettings,
            IWorkContextAccessor workContextAccessor,
            IRepository<UserRolesPartRecord> userRolesRepository,
            ICacheManager cacheManager,
            ISignals signals,
            IContentManager contentManager,
            IHttpContextAccessor httpContextAccessor)
            : base(navigationProviders, menuProviders, authorizationService,
                  navigationFilters, urlHelper, orchardServices, shellSettings) {

            _workContextAccessor = workContextAccessor;
            _userRolesRepository = userRolesRepository;
            _cacheManager = cacheManager;
            _signals = signals;
            _contentManager = contentManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public new IEnumerable<MenuItem> BuildMenu(IContent menu) {
            // Some calls to this should not be serving cached results:
            // - Calls on the admin side.
            // - Calls to some menus whose items depend on the specific request.
            // - ...
            if (ShouldCache(menu)) {
                var cachekey = GetMenuCacheKey(menu.Id);
                var cachedMenuItems = _cacheManager.Get(
                    cachekey,
                    ctx => {
                        ctx.Monitor(_signals.When("NavigationContentItems.Changed"));
                        return base.BuildMenu(menu).ToArray();
                    });

                return TryGetRouteValues(Clone(cachedMenuItems)).ToArray();
            }
            // When we are not serving from cache, let the usual NavigationManager handle everything.
            return base.BuildMenu(menu);
        }

        private bool ShouldCache(IContent menu) {
            if (AdminFilter.IsApplied(_httpContextAccessor.Current().Request.RequestContext)) {
                return false;
            }
            var cacheSettings = menu.As<CacheableMenuSettingsPart>();
            if (cacheSettings != null) {
                return cacheSettings.IsFrontEndCacheable;
            }
            return true;
        }

        private string GetMenuCacheKey(int menuId) {
            var cacheKey = string.Format("MenuWidgetPartDriverCacheKey_{0}", menuId);
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
                            original.Content,
                            $"MenuWidgetPartDriverContentCacheKey_{original.Content.Id}"),
                    Classes = original.Classes
                });
        }

        private IContent GetCachedMenuPartContent(IContent originalContent, string cacheKey) {

            var cachedMenuPartContent = _cacheManager.Get(
              cacheKey,
              ctx => {
                  ctx.Monitor(_signals.When("NavigationContentItems.Changed"));
                  var ci = _contentManager.Get(originalContent.Id,
                      VersionOptions.Number(originalContent.ContentItem.Version));
                  var cType = originalContent.GetType();
                  if (cType.GetInterface(nameof(IContent)) != null) {
                      return ci == null
                        ? (IContent)null // ideally we shouldn't be falling in this condition
                        : ci.Get(cType);
                  }
                  else {
                      return ci.As<IContent>();
                  }
              });
            return cachedMenuPartContent;

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
            var httpContext = _httpContextAccessor.Current();

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