using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.RelationshipsEnhancer")]
    public class ContentPickerConnectorPart : ContentPart {

        internal LazyField<IEnumerable<ParentContent>> _parentContent = new LazyField<IEnumerable<ParentContent>>();
        public IEnumerable<ParentContent> ParentContentItems {
            get {
                return _parentContent.Value;
            }
        }


    }

    public class ParentContent {
        public IEnumerable<string> FieldNames { get; set; }
        public ContentItem ParentContentItem { get; set; }
    }
    //[OrchardFeature("Laser.Orchard.StartupConfig.RelationshipsEnhancer")]
    //public class ContentPickerConnectorSettingsRecord  {
    //    public virtual string ContentTypesFilter { get; set; }
    //}
}