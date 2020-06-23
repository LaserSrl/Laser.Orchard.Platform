using Newtonsoft.Json;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class TerritoryTag {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("depth")]
        public int Depth { get; set; }

        [JsonProperty("selected")]
        public int Selected { get; set; }

    }
}