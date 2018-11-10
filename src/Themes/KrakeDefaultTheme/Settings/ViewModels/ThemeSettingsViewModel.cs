using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KrakeDefaultTheme.Settings.ViewModels {
    public class ThemeSettingsViewModel {
        [MaxLength(120)]
        public string BaseLineText { get; set; }
        [MaxLength(500)]
        public string HeaderLogoUrl { get; set; }
        [MaxLength(500)]
        public string PlaceholderLogoUrl { get; set; }
    }

}