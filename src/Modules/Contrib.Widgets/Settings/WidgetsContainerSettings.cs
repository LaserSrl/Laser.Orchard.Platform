using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Contrib.Widgets.Settings {
    public class WidgetsContainerSettings {
        public WidgetsContainerSettings() {
            UseHierarchicalAssociation = false;
            HierarchicalAssociationJson = "";
            TryToLocalizeItems = true;
        }
        public string AllowedZones { get; set; }
        public string AllowedWidgets { get; set; }
        public bool UseHierarchicalAssociation { get; set; }
        public string HierarchicalAssociationJson { get; set; }
        public List<Zone> HierarchicalAssociation {
            get {
                return new JavaScriptSerializer().Deserialize<List<Zone>>(HierarchicalAssociationJson);
            }
        }
        public bool TryToLocalizeItems { get; set; }
    }
}