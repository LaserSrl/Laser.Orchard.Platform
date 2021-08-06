using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class TerritoryAdministrativeTypePart :
        ContentPart<TerritoryAdministrativeTypePartRecord> {
        // This will tell us whether a territory is a Country, Province, City, None
        public TerritoryAdministrativeType AdministrativeType {
            get { return Retrieve(r => r.AdministrativeType); }
            set { Store(r => r.AdministrativeType, value); }
        }

        public virtual bool HasCities {
            get { return Retrieve(r => r.HasCities); }
            set { Store(r => r.HasCities, value); }
        }
        public virtual bool HasProvinces {
            get { return Retrieve(r => r.HasProvinces); }
            set { Store(r => r.HasProvinces, value); }
        }

    }
}