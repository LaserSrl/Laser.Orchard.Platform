using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.AdvancedSettings.ViewModels {
    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeSkinsPartEditViewModel {
        public string SelectedSkinName { get; set; }
        public IEnumerable<string> AvailableSkinNames { get; set; }

        public List<SelectListItem> Options { get; set; }
    }
}