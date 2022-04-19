using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class TerritoryISO3166CodePart : ContentPart<TerritoryISO3166CodePartRecord> {
        public string ISO3166Code {
            get { return Retrieve(r => r.ISO3166Code); }
            set { Store(r => r.ISO3166Code, value); }
        }
    }
}