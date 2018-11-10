using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.CulturePicker.Models {
    [OrchardFeature("Laser.Orchard.CulturePicker.TranslateMenuItems")]
    public class TranslateMenuItemsPart : ContentPart<TranslateMenuItemsPartRecord> {
        public bool ToBeTranslated { 
            get { return this.Retrieve(x => x.ToBeTranslated); }
            set { this.Store(x => x.ToBeTranslated, value); }
        }
        public bool Translated {
            get { return this.Retrieve(x => x.Translated); }
            set { this.Store(x => x.Translated, value); }
        }
        public string FromLocale {
            get { return this.Retrieve(x => x.FromLocale); }
            set { this.Store(x => x.FromLocale, value); }
        }
    }

    [OrchardFeature("Laser.Orchard.CulturePicker.TranslateMenuItems")]
    public class TranslateMenuItemsPartRecord : ContentPartRecord {
        public virtual bool ToBeTranslated { get; set; }
        public virtual bool Translated { get; set; }
        public virtual string FromLocale { get; set; }
    }
}