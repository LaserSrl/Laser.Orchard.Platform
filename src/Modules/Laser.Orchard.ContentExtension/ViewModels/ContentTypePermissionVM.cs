using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ContentExtension.ViewModels {
    public class ContentTypePermissionVM {
        public string ContentType { get; set; }
        public string Post { get; set; }
        public string Get { get; set; }
        public string Delete { get; set; }
    }
}