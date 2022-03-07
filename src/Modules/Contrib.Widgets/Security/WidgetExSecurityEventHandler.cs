using Contrib.Widgets.Models;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Settings;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core = Orchard.Core;

namespace Contrib.Widgets.Security {
    public class WidgetExSecurityEventHandler : IAuthorizationServiceEventHandler {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WidgetExSecurityEventHandler(
            IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Adjust(CheckAccessContext context) {
            // when checking permissions for WidgetExPart inside a WidgetContainer
            // use the permission to ManageWidgetContainer 

            // If we haven't yet granted the user permission to do whatever they are trying to, and
            // we are testing for one of the "manage" permissions from Orchard.Core.Contents:
            if (!context.Granted && IsCoreManagePermission(context.Permission)) {
                // We are only going to worry about widgets that aren't securable, because
                // those we assume will have been configured properly. If they haven't, we have
                // other levers to handle that precisely anyway.
                var typeDefinition = context.Content.ContentItem.TypeDefinition;
                if (!typeDefinition.Settings.GetModel<ContentTypeSettings>().Securable) {
                    // We are only going to worry about widgets
                    if (context.Content.ContentItem.Is<WidgetExPart>()) {
                        // Checking for the ability for a user to manage this widget.
                        // All this we are doing doesn't apply on the page/controller for a site's widgets
                        // but only for widgets within a WidgetsContainer.
                        if (!RequestIsInWidgetsAdmin()) {
                            context.Granted = false;
                            context.Adjusted = true;
                            context.Permission = Permissions.ManageContainerWidgets;
                        }
                    }
                }

            }
        }

        // Is the permission any of the "backoffice" permissions from Orchard.Core.Contents?
        private bool IsCoreManagePermission(Permission perm) {
            return perm == Core.Contents.Permissions.CreateContent
                || perm == Core.Contents.Permissions.PublishContent
                || perm == Core.Contents.Permissions.PublishOwnContent
                || perm == Core.Contents.Permissions.EditContent
                || perm == Core.Contents.Permissions.EditOwnContent
                || perm == Core.Contents.Permissions.DeleteContent
                || perm == Core.Contents.Permissions.DeleteOwnContent;
        }

        private bool RequestIsInWidgetsAdmin() {
            var routeData = _httpContextAccessor.Current().Request.RequestContext.RouteData;
            return routeData.Values.ContainsKey("Area")
                && routeData.Values["Area"].ToString().Equals("Orchard.Widgets", StringComparison.OrdinalIgnoreCase)
                && routeData.Values.ContainsKey("Controller")
                && routeData.Values["Controller"].ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        public void Checking(CheckAccessContext context) {
        }

        public void Complete(CheckAccessContext context) {
        }
    }
}