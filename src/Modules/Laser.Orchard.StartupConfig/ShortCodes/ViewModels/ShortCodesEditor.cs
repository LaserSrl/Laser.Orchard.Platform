using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.ShortCodes.Abstractions;

namespace Laser.Orchard.StartupConfig.ShortCodes.ViewModels {

    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class ShortCodesEditor {
        public string ContentType { get; set; }
        public ContentPart Part { get; set; }
        public ContentField Field { get; set; }
        public string ElementName { get; set; }
        public string ElementFlavor { get; set; }
        public IEnumerable<Descriptor> Descriptors { get; set; }
    }
}