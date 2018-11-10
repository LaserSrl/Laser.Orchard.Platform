using Laser.Orchard.Highlights.Enums;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Highlights.Models {

    public class HighlightsItemPartRecord : ContentPartRecord {

        public virtual bool Video { get; set; }
        public virtual string Sottotitolo { get; set; }
        public virtual string TitleSize { get; set; }
        public virtual int HighlightsGroupPartRecordId { get; set; }
        public virtual string LinkUrl { get; set; }
        public virtual LinkTargets LinkTarget { get; set; }
        public virtual string LinkText { get; set; }
        public virtual int ItemOrder { get; set; }
    }
}