using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.TemplateManagement.Models {
    public class SiteSettingsPart : ContentPart<SiteSettingsPartRecord> {
        public string DefaultParserIdSelected {
            get { return Record.DefaultParserIdSelected; }
            set { Record.DefaultParserIdSelected = value; }
        }
    }

    public class SiteSettingsPartRecord : ContentPartRecord {
        public virtual string DefaultParserIdSelected { get; set; }
    }
}