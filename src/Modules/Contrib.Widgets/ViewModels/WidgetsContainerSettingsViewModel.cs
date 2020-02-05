using Contrib.Widgets.Settings;
using System.Collections.Generic;

namespace Contrib.Widgets.ViewModels {
    public class WidgetsContainerSettingsViewModel {
        public string[] SelectedZones { get; set; }
        public List<string> Zones { get; set; }
        public string[] SelectedWidgets { get; set; }
        public List<string> Widgets { get; set; }
        public bool UseHierarchicalAssociation { get; set; }
        public string HierarchicalAssociationJson { get; set; }
        public bool TryToLocalizeItems { get; set; }
    }
}