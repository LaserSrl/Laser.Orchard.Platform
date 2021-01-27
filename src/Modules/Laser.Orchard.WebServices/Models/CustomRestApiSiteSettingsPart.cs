using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Laser.Orchard.WebServices.Models {
    [OrchardFeature("Laser.Orchard.CustomRestApi")]
    public class CustomRestApiSiteSettingsPart : ContentPart {

        public string ConfigurationJson {
            get { return this.Retrieve(p => p.ConfigurationJson); }
            set { this.Store(p => p.ConfigurationJson, value); }
        }

        // TODO: this class isn't very robust as it is, meaning it doesn't enforce
        // much validation. There is some in the driver, but that's it.
        public IEnumerable<RestApiAction> GetActionsConfiguration() {
            return JsonConvert
                .DeserializeObject<RestApiAction[]>(ConfigurationJson);
        }
    }
}