using Laser.Orchard.WebServices.Models;
using Newtonsoft.Json;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Laser.Orchard.WebServices.ViewModels {
    [OrchardFeature("Laser.Orchard.CustomRestApi")]
    public class CustomRestApiSiteSettingsPartViewModel {

        public CustomRestApiSiteSettingsPartViewModel () {
            ConfigurationJson = "[]";
        }

        public CustomRestApiSiteSettingsPartViewModel (
            CustomRestApiSiteSettingsPart part) : this() {

            if (!string.IsNullOrWhiteSpace(part.ConfigurationJson)) {
                ConfigurationJson = part.ConfigurationJson;
            }
        }

        public string ConfigurationJson { get; set; }
        

        public IEnumerable<RestApiAction> GetActionsConfiguration() {
            return JsonConvert
                .DeserializeObject<RestApiAction[]>(
                    ConfigurationJson);
        }
    }
}