using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ShortCodes.Abstractions {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class DescribeContext {
        public DescribeContext() {
            Host = new HostInfos();
        }

        public HostInfos Host { get; set; }
    }

    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class HostInfos {
        public int ContentId { get; set; }
        public string ContentType { get; set; }
        public ContentPart Part { get; set; }
        public ContentField Field { get; set; }
    }
}