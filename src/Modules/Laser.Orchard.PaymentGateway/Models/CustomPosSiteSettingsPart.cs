using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.PaymentGateway.Models {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public class CustomPosSiteSettingsPart : ContentPart {
        public List<CustomPosSettings> CustomPos {
            get {
                if (string.IsNullOrWhiteSpace(this.CustomPosSerialized)) {
                    return new List<CustomPosSettings>();
                } else {
                    return JsonConvert.DeserializeObject<List<CustomPosSettings>>(this.CustomPosSerialized);
                }
            }
            set { this.CustomPosSerialized = JsonConvert.SerializeObject(value
                .OrderBy(cps => cps.Order).ToList()); }
        }

        public string CustomPosSerialized {
            get { return this.Retrieve(x => x.CustomPosSerialized); }
            set { this.Store(x => x.CustomPosSerialized, value); }
        }
    }
}