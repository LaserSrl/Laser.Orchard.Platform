using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents.Settings;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Taxonomies;
using Orchard.Taxonomies.Models;
using Orchard.Widgets.Models;
using Core = Orchard.Core;
namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.Security {
    public class WidgetsFrontEndAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        public void Checking(CheckAccessContext context) {
        }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            Permission permission = context.Permission;
            if (context.Content.Is<ICommonPart>()) {
                var typeDefinition = context.Content.ContentItem.TypeDefinition;
                // adjusting permissions only if the content is not securable
                if (!typeDefinition.Settings.GetModel<ContentTypeSettings>().Securable) {
                    string stereotype;
                    if (context.Permission == Core.Contents.Permissions.ViewContent || context.Permission == Core.Contents.Permissions.ViewOwnContent) {
                        if (context.Content.Is<WidgetPart>()) {
                            context.Granted = true;
                            context.Adjusted = false;
                        }
                        else if (context.Content != null &&
                                    ((context.Content.ContentItem.TypeDefinition.Settings.TryGetValue("Stereotype", out stereotype) && stereotype == "MenuItem") ||
                                    context.Content.ContentItem.ContentType == "Menu")) {
                            context.Granted = true;
                            context.Adjusted = false;
                        }
                    }
                }
            }
        }
    }
}