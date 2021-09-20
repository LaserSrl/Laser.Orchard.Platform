using Newtonsoft.Json;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdvancedSettings.ViewModels {

    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class SkinsManifest {
        public SkinsManifest() {
            Skins = Enumerable.Empty<ThemeSkinDescription>().ToArray();
            Variables = Enumerable.Empty<ThemeCssVariable>().ToArray();
        }

        [JsonProperty("skins", NullValueHandling = NullValueHandling.Ignore)]
        public ThemeSkinDescription[] Skins { get; set; }
        [JsonProperty("variables", NullValueHandling = NullValueHandling.Ignore)]
        public ThemeCssVariable[] Variables { get; set; }
    }

    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeSkinDescription {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("stylesheets", NullValueHandling = NullValueHandling.Ignore)]
        public string[] StyleSheets { get; set; }
        [JsonProperty("headscripts", NullValueHandling = NullValueHandling.Ignore)]
        public string[] HeadScripts { get; set; }
        [JsonProperty("footscripts", NullValueHandling = NullValueHandling.Ignore)]
        public string[] FootScripts { get; set; }
    }

    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeCssVariable {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }
        [JsonProperty("displayname", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }
        [JsonProperty("variabletype", NullValueHandling = NullValueHandling.Ignore)]
        public string VariableType { get; set; }
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
        
    }
}