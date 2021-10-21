using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Laser.Orchard.PaymentGateway.Models {
    public class CustomPosSiteSettingsPart : ContentPart {
        public List<CustomPosSettings> CustomPos {
            get { return this.Retrieve(x => x.CustomPos); }
            set { this.Store(x => x.CustomPos, value); }
        }
    }
}