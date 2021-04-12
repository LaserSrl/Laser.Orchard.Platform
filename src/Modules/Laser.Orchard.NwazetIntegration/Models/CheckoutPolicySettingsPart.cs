using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class CheckoutPolicySettingsPart : ContentPart {

        public string[] PolicyTextReferences {
            get {
                return (Retrieve<string>("PolicyTextReferences") ?? "")
                    .Split(new[] { ',', ' ' },
                    StringSplitOptions.RemoveEmptyEntries);
            }
            set { Store("PolicyTextReferences", String.Join(", ", value)); }
        }
    }
}