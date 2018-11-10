using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Laser.Orchard.CulturePicker.Models {
    public class CulturePickerPart : ContentPart {
        public IList<ExtendedCultureRecord> AvailableCultures { get; set; }
        public IList<ExtendedCultureRecord> TranslatedCultures { get; set; }
        public bool ShowOnlyPertinentCultures { get; set; }
        public bool ShowLabel { get; set; }
        public ExtendedCultureRecord UserCulture { get; set; }
    }
}