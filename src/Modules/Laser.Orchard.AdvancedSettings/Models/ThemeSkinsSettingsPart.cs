using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;

namespace Laser.Orchard.AdvancedSettings.Models {
    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeSkinsSettingsPart : ContentPart {
        public static string AllSkinsValue = "{All}";
        public static string EditorGroupId = "ThemeSkins";
        public string[] AvailableSkinNames {
            get {
                return (Retrieve<string>("AvailableSkinNames") ?? string.Empty)
                    .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            set {
                Store("AvailableSkinNames", string.Join(",", value));
            }
        }
    }
}