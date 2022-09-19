using Newtonsoft.Json;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        [JsonConverter(typeof(SkinNameConverter))]
        public string Name { get; set; }

        [JsonProperty("stylesheets", NullValueHandling = NullValueHandling.Ignore)]
        public string[] StyleSheets { get; set; }
        [JsonProperty("headscripts", NullValueHandling = NullValueHandling.Ignore)]
        public string[] HeadScripts { get; set; }
        [JsonProperty("footscripts", NullValueHandling = NullValueHandling.Ignore)]
        public string[] FootScripts { get; set; }
        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
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

    class SkinNameConverter : JsonConverter<string> {
        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var name = (string)reader.Value;
            // replace invalid characters:
            // name can't contain commas, to simplify serialization
            // name can't contain curly braces, to reserve special names
            name = name.Replace(',', ' ').Replace('{', ' ').Replace('}', ' ');
            // remove whitespace at start/end of name
            name = name.Trim();
            return name;
        }

        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer) {
            // TODO: currently we only use this converter for reading
            throw new NotImplementedException();
        }
    }
}