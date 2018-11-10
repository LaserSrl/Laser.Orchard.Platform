using Laser.Orchard.Highlights.Enums;
using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;
using Laser.Orchard.Highlights.ViewModels;
using Orchard.Data;

namespace Laser.Orchard.Highlights.Models {

    public class HighlightsItemPart : ContentPart<HighlightsItemPartRecord> {

        public bool Video {
            get { return Record.Video; }
            set { Record.Video = value; }
        }

        public string Sottotitolo {
            get { return Record.Sottotitolo; }
            set { Record.Sottotitolo = value; }
        }

        public string TitleSize {
            get { return Record.TitleSize; }
            set { Record.TitleSize = value; }
        }

        [Required]
        public int HighlightsGroupPartRecordId {
            get { return Record.HighlightsGroupPartRecordId; }
            set { Record.HighlightsGroupPartRecordId = value; }
        }

        public string LinkUrl {
            get { return Record.LinkUrl; }
            set { Record.LinkUrl = value; }
        }

        public LinkTargets LinkTarget {
            get { return Record.LinkTarget; }
            set { Record.LinkTarget = value; }
        }

        public string LinkText {
            get { return Record.LinkText; }
            set { Record.LinkText = value; }
        }

        public int ItemOrder {
            get { return Record.ItemOrder; }
            set { Record.ItemOrder = value; }
        }

        #region [ Properties without a Record ]
        public string GroupShapeName { get; set; }
        public string GroupDisplayPlugin { get; set; }
        public Enums.DisplayTemplate GroupDisplayTemplate { get; set; }
        #endregion
    }
}