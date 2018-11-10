using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents.Settings;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Users.Models;
using System.Collections.Generic;
using OrchardCore = Orchard.Core;
using System.Linq;
using System.Web.Mvc;
using Orchard.Mvc.Filters;
using System.Diagnostics.CodeAnalysis;


namespace Laser.Orchard.StartupConfig.Security {
    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    // [ExcludeFromCodeCoverage]
    public class GroupsAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IUsersGroupsServices _usersGroupsServices;
        public GroupsAuthorizationEventHandler(IUsersGroupsServices usersGroupsServices) {
            _usersGroupsServices = usersGroupsServices;
        }

        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }
        public void Adjust(CheckAccessContext context) {
            //if (context.Permission.Name.Equals("ManageMediaContent")) {
            //    context.Adjusted = true;
            //    context.Permission = GroupsPermissions.ViewMedia;
            //}
            
            if (!context.Granted &&
                context.Content.Is<ICommonPart>()) {

                if (HasGroupOwnership(context.User, context.Content)) {

                    if (context.Permission.Name.Equals("EditContent")) {
                        context.Adjusted = true;
                        context.Permission = GroupsPermissions.EditContentForOwnGroups;
                    }
                    else if (context.Permission.Name.Equals("PublishContent")) {
                        context.Adjusted = true;
                        context.Permission = GroupsPermissions.PublishContentForOwnGroups;
                    }
                    else if (context.Permission.Name.Equals("DeleteContent")) {
                        context.Adjusted = true;
                        context.Permission = GroupsPermissions.DeleteContentForOwnGroups;
                    } else if (context.Permission.Name.Equals("ManageBlogs")) {
                        context.Adjusted = true;
                        context.Permission = GroupsPermissions.ManageBlogsForOwnGroups;

                    } else if (context.Permission.Name.Equals("PublishBlogPost")) {
                        context.Adjusted = true;
                        context.Permission = GroupsPermissions.PublishBlogPostForOwnGroups;

                    } else if (context.Permission.Name.Equals("EditBlogPost")) {
                        context.Adjusted = true;
                        context.Permission = GroupsPermissions.EditBlogPostForOwnGroups;

                    } else if (context.Permission.Name.Equals("DeleteBlogPost")) {
                        context.Adjusted = true;
                        context.Permission = GroupsPermissions.DeleteBlogPostForOwnGroups;

                    } else if (context.Permission.Name.StartsWith("Delete_") || context.Permission.Name.StartsWith("Edit_") || context.Permission.Name.StartsWith("Publish_")) {
                        var permission = context.Permission;
                        permission = GetGroupVariation(context.Permission); // output is like PublishGroup_{0}

                        var typeDefinition = context.Content.ContentItem.TypeDefinition;
                        if (typeDefinition.Settings.GetModel<ContentTypeSettings>().Securable) {
                            if (permission != null) {
                                context.Adjusted = true;
                                context.Permission = DynamicGroupsPermissions.CreateDynamicGroupPermission(permission, typeDefinition);
                            }
                        }

                    }

                }
            }
        }

        private bool HasGroupOwnership(IUser user, global::Orchard.ContentManagement.IContent content) {
            if (user == null || content == null)
                return false;

            var common = content.As<ICommonPart>();
            if (common == null || common.Owner == null)
                return false;
            return (_usersGroupsServices.SameGroup(common.Owner.Id, user.Id));

        }

        private static Permission GetGroupVariation(Permission permission) {
            return DynamicGroupsPermissions.ConvertToGroupPermission(permission);
        }

    }
}
