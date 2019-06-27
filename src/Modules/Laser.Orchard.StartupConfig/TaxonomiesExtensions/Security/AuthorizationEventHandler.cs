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
using Core = Orchard.Core;
namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.Security {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        public void Checking(CheckAccessContext context) {
            Permission permission = context.Permission;
            if (context.Content.Is<ICommonPart>()) {
                var typeDefinition = context.Content.ContentItem.TypeDefinition;
                // adjusting permissions only if the content is not securable
                if (!typeDefinition.Settings.GetModel<ContentTypeSettings>().Securable) {
                    if (context.Content.Is<TermPart>()) {
                        if (context.Permission == Core.Contents.Permissions.CreateContent) {
                            permission = Permissions.CreateTerm;
                        }
                        else if (context.Permission == Core.Contents.Permissions.EditContent) {
                            permission = Permissions.EditTerm;
                        }
                        else if (context.Permission == Core.Contents.Permissions.PublishContent) {
                            permission = Permissions.EditTerm;
                        }
                        else if (context.Permission == Core.Contents.Permissions.DeleteContent) {
                            permission = Permissions.DeleteTerm;
                        }
                    }
                    else if (context.Content.Is<TaxonomyPart>()) {
                        if (context.Permission == Core.Contents.Permissions.CreateContent) {
                            permission = Permissions.CreateTaxonomy;
                        }
                        else if (context.Permission == Core.Contents.Permissions.EditContent) {
                            permission = Permissions.CreateTaxonomy;
                        }
                        else if (context.Permission == Core.Contents.Permissions.PublishContent) {
                            permission = Permissions.CreateTaxonomy;
                        }
                        else if (context.Permission == Core.Contents.Permissions.DeleteContent) {
                            permission = Permissions.ManageTaxonomies;
                        }
                    }
                    if (permission != context.Permission) {
                        context.Granted = false; //Force granted to false so next adjust iteration will check against the new permission starting from an unauthorized condition
                        context.Permission = permission;
                        context.Adjusted = true;
                    }
                }
            }
        }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
        }
    }
}