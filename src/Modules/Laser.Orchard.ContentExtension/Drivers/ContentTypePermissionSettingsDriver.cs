using Laser.Orchard.ContentExtension.Models;
using Laser.Orchard.ContentExtension.Services;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using System.Xml.Linq;
using System.Linq;

namespace Laser.Orchard.ContentExtension.Drivers {
    public class ContentTypePermissionSettingsDriver : ContentPartDriver<ContentTypePermissionSettingsPart> {
        private readonly IContentTypePermissionSettingsService _contentTypePermissionSettingsService;
        public ContentTypePermissionSettingsDriver(IContentTypePermissionSettingsService contentTypePermissionSettingsService) {
            _contentTypePermissionSettingsService = contentTypePermissionSettingsService;
        }
        protected override void Exporting(ContentTypePermissionSettingsPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            var settings = _contentTypePermissionSettingsService.ReadSettings();
            foreach(var perm in settings.ListContPermission) {
                XElement permission = new XElement("Permission");
                permission.SetAttributeValue("ContentType", perm.ContentType);
                permission.SetAttributeValue("PostPermission", perm.PostPermission);
                permission.SetAttributeValue("GetPermission", perm.GetPermission);
                permission.SetAttributeValue("DeletePermission", perm.DeletePermission);
                permission.SetAttributeValue("PublishPermission", perm.PublishPermission);
                root.Add(permission);
            }
        }
        protected override void Importing(ContentTypePermissionSettingsPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            if(root == null) {
                return;
            }
            var permissions = root.Elements("Permission");
            var settings = _contentTypePermissionSettingsService.ReadSettings();
            foreach(var perm in permissions) {
                var permRecord = new ContentTypePermissionRecord {
                    ContentType = perm.Attribute("ContentType") != null ? perm.Attribute("ContentType").Value : "",
                    PostPermission = perm.Attribute("PostPermission") != null ? perm.Attribute("PostPermission").Value : "",
                    GetPermission = perm.Attribute("GetPermission") != null ? perm.Attribute("GetPermission").Value : "",
                    DeletePermission = perm.Attribute("DeletePermission") != null ? perm.Attribute("DeletePermission").Value : "",
                    PublishPermission = perm.Attribute("PublishPermission") != null ? perm.Attribute("PublishPermission").Value : ""
                };
                // insert record only if not already inserted before in the system
                var check = settings.ListContPermission.Count(x => x.ContentType == permRecord.ContentType
                    && x.PostPermission == permRecord.PostPermission && x.GetPermission == permRecord.GetPermission
                    && x.PublishPermission == permRecord.PublishPermission && x.DeletePermission == permRecord.DeletePermission);
                if(check == 0) {
                    settings.ListContPermission.Add(permRecord);
                }
            }
            _contentTypePermissionSettingsService.WriteSettings(settings);
        }
    }
}