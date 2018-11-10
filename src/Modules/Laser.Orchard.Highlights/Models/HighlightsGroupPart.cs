using Laser.Orchard.Highlights.Enums;
using Orchard.ContentManagement;

namespace Laser.Orchard.Highlights.Models {

    public class HighlightsGroupPart : ContentPart<HighlightsGroupPartRecord> {

        public string DisplayPlugin {
            get { return Record.DisplayPlugin; }
            set { Record.DisplayPlugin = value; }
        }

        public DisplayTemplate DisplayTemplate {
            get { return Record.DisplayTemplate; }
            set { Record.DisplayTemplate = value; }
        }
        public ItemsSourceTypes ItemsSourceType {
            get { return Record.ItemsSourceType; }
            set { Record.ItemsSourceType = value; }
        }
        public int Query_Id {
            get { return Record.Query_Id; }
            set { Record.Query_Id = value; }
        }
    }
}