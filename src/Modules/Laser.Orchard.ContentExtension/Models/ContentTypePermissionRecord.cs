using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ContentExtension.Models {

    public class ContentTypePermissionRecord {
        public ContentTypePermissionRecord() {
            Id = 0;
        }
        public virtual int Id { get; set; }
        public virtual string ContentType { get; set; }
        public virtual string PostPermission { get; set; }
        public virtual string GetPermission { get; set; }
        public virtual string DeletePermission { get; set; }
        public virtual string PublishPermission { get; set; }
    }
    public class SettingsModel {
         public virtual IList<ContentTypePermissionRecord> ListContPermission { get; set; }
    }
    /// <summary>
    /// This is an empty class. It is usefull to manage import and export of settings.
    /// </summary>
    public class ContentTypePermissionSettingsPart : ContentPart {
    }
}