using Newtonsoft.Json;
using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Laser.Orchard.PaymentGateway.Models {
    public class CustomPosSiteSettingsPart : ContentPart {
        public List<CustomPosSettings> CustomPos {
            get {
                if (string.IsNullOrWhiteSpace(this.CustomPosSerialized)) {
                    return new List<CustomPosSettings>();
                } else {
                    return JsonConvert.DeserializeObject<List<CustomPosSettings>>(this.CustomPosSerialized);
                }
            }
            set { this.CustomPosSerialized = JsonConvert.SerializeObject(value); }
        }

        public string CustomPosSerialized {
            get { return this.Retrieve(x => x.CustomPosSerialized); }
            set { this.Store(x => x.CustomPosSerialized, value); }
        }
    }
}