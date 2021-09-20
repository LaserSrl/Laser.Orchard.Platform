using Laser.Orchard.AdvancedSettings.ViewModels;
using Newtonsoft.Json;
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

        public string SerializedVariables {
            get { return this.Retrieve(x => x.SerializedVariables); }
            set { this.Store(x => x.SerializedVariables, value); }
        }

        public ThemeCssVariable[] Variables {
            get {
                return JsonConvert.DeserializeObject<ThemeCssVariable[]>(SerializedVariables ?? "");
            }
            set {
                SerializedVariables = JsonConvert.SerializeObject(value);
            }
        }
    }
}