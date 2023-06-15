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
            Skins = Enumerable.Empty<ThemeSkinDescription>().ToList();
            Variables = Enumerable.Empty<ThemeCssVariable>().ToList();
        }

        [JsonProperty("skins", NullValueHandling = NullValueHandling.Ignore)]
        public List<ThemeSkinDescription> Skins { get; set; }
        [JsonProperty("variables", NullValueHandling = NullValueHandling.Ignore)]
        public List<ThemeCssVariable> Variables { get; set; }

        public static SkinsManifest MergeManifests(IEnumerable<SkinsManifest> manifests) {
            var cssVariablesComparer = new ThemeCssVariableComparer();
            // The input to this is assumed to ordered so that the first is more specific.
            // (i.e. the first manifest passed is from the current theme, the second is from
            // its base theme...)
            var currentThemeManifest = manifests.FirstOrDefault();

            foreach (var manifest in manifests.Skip(1)) {
                // Merge skin descriptions: parent resources go first
                foreach (var skin in manifest.Skins) {
                    // The "Identifier" for the skin is its name. See if the current theme declares a
                    // skin by the same name.
                    var currentSkin = currentThemeManifest.Skins
                        .FirstOrDefault(s => s.Name.Equals(skin.Name));
                    if (currentSkin != null) {
                        // In this case, the resources from the parent go before the resources from the child
                        // (i.e. the "new" resources from the skin we are processing go before the ones we
                        // already have in our object), except where they are re-inserted in the child.
                        currentSkin.StyleSheets.InsertRange(0,
                            skin.StyleSheets.Except(currentSkin.StyleSheets));
                        currentSkin.HeadScripts.InsertRange(0,
                            skin.HeadScripts.Except(currentSkin.HeadScripts));
                        currentSkin.FootScripts.InsertRange(0,
                            skin.FootScripts.Except(currentSkin.FootScripts));
                    } else {
                        // Add the skin
                        currentThemeManifest.Skins.Add(skin);
                    }
                }
                // Merge css variables: parent resources go first, unless we redefine them in the child.
                // The comparison between css vriables is based on their name.
                currentThemeManifest.Variables.InsertRange(0, 
                    manifest.Variables.Except(
                        currentThemeManifest.Variables, cssVariablesComparer));
            }

            return currentThemeManifest;
        }
    }

    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeSkinDescription {
        [JsonProperty("name", Required = Required.Always)]
        [JsonConverter(typeof(SkinNameConverter))]
        public string Name { get; set; }

        [JsonProperty("stylesheets", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> StyleSheets { get; set; }
        [JsonProperty("headscripts", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> HeadScripts { get; set; }
        [JsonProperty("footscripts", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> FootScripts { get; set; }
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

    class ThemeCssVariableComparer : IEqualityComparer<ThemeCssVariable> {
        public bool Equals(ThemeCssVariable x, ThemeCssVariable y) {
            if (x == null && y == null) {
                return true;
            }
            if (x == null || y == null) {
                return false;
            }
            return string.Equals(x.Name, y.Name);
        }

        public int GetHashCode(ThemeCssVariable obj) {
            return obj.Name.GetHashCode();
        }
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