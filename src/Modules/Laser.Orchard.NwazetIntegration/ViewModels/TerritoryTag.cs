using Newtonsoft.Json;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class TerritoryTag {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}