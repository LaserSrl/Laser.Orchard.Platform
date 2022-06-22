using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Models {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointPartRecord : ContentPartRecord {

        public virtual string CountryName { get; set; }
        public virtual int CountryId { get; set; }

        public virtual string ProvinceName { get; set; }
        public virtual int ProvinceId { get; set; }

        public virtual string CityName { get; set; }
        public virtual int CityId { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string PostalCode { get; set; }

    }
}