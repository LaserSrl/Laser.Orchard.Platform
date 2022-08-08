using Newtonsoft.Json;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class TerritoryTag {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }


        private string[] vSplit {
            get {
                return Value.Split(';');
            }
        }

        [JsonIgnore]
        public int InternalId {
            get {
                var str = vSplit != null && vSplit.Length > 0 ? vSplit[0] : "";
                int tmp = 0;
                if (int.TryParse(vSplit[0], out tmp)) {
                    return tmp;
                }
                return 0;
            }
        }

        [JsonIgnore]
        public string NameHash {
            get {
                return vSplit != null && vSplit.Length > 1 ? vSplit[1] : "";
            }
        }

    }
}