using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Widgets.Models;

namespace Contrib.Widgets.Models {
    public class WidgetExPart : ContentPart<WidgetExPartRecord> {
        internal Orchard.ContentManagement.Utilities.LazyField<ContentItem> HostField = new Orchard.ContentManagement.Utilities.LazyField<ContentItem>();

        public ContentItem Host {
            get { return HostField.Value; }
            set { HostField.Value = value; }
        }

        public string Zone {
            get {
                if (this.As<WidgetPart>() != null) {
                    return this.As<WidgetPart>().Zone;
                } else { 
                    return ""; 
                }
            }
        }

        public string Position {
            get {
                if (this.As<WidgetPart>() != null) {
                    return this.As<WidgetPart>().Position;
                } else {
                    return "";
                }
            }
        }

        public bool IsPublished {
            get {
                if (this.As<WidgetPart>() != null) {
                    return this.As<WidgetPart>().HasPublished();
                } else {
                    return false;
                }
            }
        }

        public bool HasDraft {
            get {
                if (this.As<WidgetPart>() != null) {
                    return this.As<WidgetPart>().HasDraft();
                } else {
                    return false;
                }
            }
        }
    }

    public class WidgetExPartRecord : ContentPartRecord {
        public virtual int? HostId { get; set; }
    }
}