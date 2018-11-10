using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Contents.Settings;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using OrchardCore = Orchard.Core;
using OrchardBlogs = Orchard.Blogs;
using Orchard.Localization;
using Orchard.ContentPermissions;


namespace Laser.Orchard.StartupConfig.Security {

    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class GroupsPermissions : IPermissionProvider {

        public static readonly Permission PublishContentForOwnGroups = new Permission { Description = "Publish or unpublish for own groups", Name = "PublishGroupsContent", ImpliedBy = new[] { OrchardCore.Contents.Permissions.PublishContent}, Category = "Contents Feature" };
        public static readonly Permission EditContentForOwnGroups = new Permission { Description = "Edit for own groups", Name = "EditGroupsContent", ImpliedBy = new[] { PublishContentForOwnGroups, OrchardCore.Contents.Permissions.EditContent }, Category = "Contents Feature" };
        public static readonly Permission DeleteContentForOwnGroups = new Permission { Description = "Delete for own groups", Name = "DeleteGroupsContent", ImpliedBy = new[] { OrchardCore.Contents.Permissions.DeleteContent }, Category = "Contents Feature" };

        // Blogs
        public static readonly Permission ManageBlogsForOwnGroups = new Permission { Description = "Manage blogs for own groups", Name = "ManageGroupBlogs", ImpliedBy = new[] { OrchardBlogs.Permissions.ManageBlogs }, Category = "Orchard.Blogs Feature" };
        public static readonly Permission PublishBlogPostForOwnGroups = new Permission { Description = "Publish or unpublish blog post for own groups", Name = "PublishGroupBlogPost", ImpliedBy = new[] { OrchardBlogs.Permissions.PublishBlogPost, ManageBlogsForOwnGroups }, Category = "Orchard.Blogs Feature" };
        public static readonly Permission EditBlogPostForOwnGroups = new Permission { Description = "Edit blog posts for own groups", Name = "EditGroupBlogPost", ImpliedBy = new[] { OrchardBlogs.Permissions.EditBlogPost, PublishBlogPostForOwnGroups }, Category = "Orchard.Blogs Feature" };
        public static readonly Permission DeleteBlogPostForOwnGroups = new Permission { Description = "Delete blog post for own groups", Name = "DeleteGroupBlogPost", ImpliedBy = new[] { OrchardBlogs.Permissions.DeleteBlogPost, ManageBlogsForOwnGroups }, Category = "Orchard.Blogs Feature" };

        public virtual Feature Feature { get; set; }

        public GroupsPermissions() {
            
            if (!OrchardCore.Contents.Permissions.EditOwnContent.ImpliedBy.Contains(EditContentForOwnGroups)){
                OrchardCore.Contents.Permissions.EditOwnContent.ImpliedBy =  OrchardCore.Contents.Permissions.EditOwnContent.ImpliedBy.Concat(new[] { EditContentForOwnGroups });
            }
            if (!OrchardCore.Contents.Permissions.PublishOwnContent.ImpliedBy.Contains(PublishContentForOwnGroups)) {
                OrchardCore.Contents.Permissions.PublishOwnContent.ImpliedBy = OrchardCore.Contents.Permissions.PublishOwnContent.ImpliedBy.Concat(new[] { PublishContentForOwnGroups });
            }
            if (!OrchardCore.Contents.Permissions.DeleteOwnContent.ImpliedBy.Contains(DeleteContentForOwnGroups)) {
                OrchardCore.Contents.Permissions.DeleteOwnContent.ImpliedBy = OrchardCore.Contents.Permissions.DeleteOwnContent.ImpliedBy.Concat(new[] { DeleteContentForOwnGroups });
            }

            //Blog
            if (!OrchardBlogs.Permissions.ManageOwnBlogs.ImpliedBy.Contains(ManageBlogsForOwnGroups)) {
                OrchardBlogs.Permissions.ManageOwnBlogs.ImpliedBy = OrchardBlogs.Permissions.DeleteOwnBlogPost.ImpliedBy.Concat(new[] { ManageBlogsForOwnGroups });
            }
            if (!OrchardBlogs.Permissions.DeleteOwnBlogPost.ImpliedBy.Contains(DeleteBlogPostForOwnGroups)) {
                OrchardBlogs.Permissions.DeleteOwnBlogPost.ImpliedBy = OrchardBlogs.Permissions.DeleteOwnBlogPost.ImpliedBy.Concat(new[] { DeleteBlogPostForOwnGroups });
            }
            if (!OrchardBlogs.Permissions.PublishOwnBlogPost.ImpliedBy.Contains(PublishBlogPostForOwnGroups)) {
                OrchardBlogs.Permissions.PublishOwnBlogPost.ImpliedBy = OrchardBlogs.Permissions.PublishOwnBlogPost.ImpliedBy.Concat(new[] { PublishBlogPostForOwnGroups });
            }
            if (!OrchardBlogs.Permissions.EditOwnBlogPost.ImpliedBy.Contains(EditBlogPostForOwnGroups)) {
                OrchardBlogs.Permissions.EditOwnBlogPost.ImpliedBy = OrchardBlogs.Permissions.EditOwnBlogPost.ImpliedBy.Concat(new[] { EditBlogPostForOwnGroups });
            }
            if (!OrchardBlogs.Permissions.MetaListBlogs.ImpliedBy.Contains(EditBlogPostForOwnGroups)) {
                OrchardBlogs.Permissions.MetaListBlogs.ImpliedBy = OrchardBlogs.Permissions.MetaListOwnBlogs.ImpliedBy.Concat(new[] { DeleteBlogPostForOwnGroups, EditBlogPostForOwnGroups, PublishBlogPostForOwnGroups });
            }
        }
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                PublishContentForOwnGroups,
                EditContentForOwnGroups,   
                DeleteContentForOwnGroups,
                ManageBlogsForOwnGroups,
                PublishBlogPostForOwnGroups, 
                EditBlogPostForOwnGroups,
                DeleteBlogPostForOwnGroups,
    
            };
        }

    }
    
}