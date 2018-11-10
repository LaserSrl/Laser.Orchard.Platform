using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Laser.Orchard.DynamicNavigation.ViewModels {

    public class DynamicMenuViewModel {

        public int MenuId { get; set; }
        public int LevelsToShow { get; set; }
        public bool ShowFirstLevelBrothers { get; set; }
        public IEnumerable<ContentItem> Menus { get; set; }
    }
}