using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdvancedSettings.Models {
    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeSkinsPart : ContentPart {

        public string SkinName {
            get { return this.Retrieve(x => x.SkinName); }
            set { this.Store(x => x.SkinName, value); }
        }
    }
}