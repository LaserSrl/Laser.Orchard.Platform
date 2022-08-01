using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Models {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointOrderPartRecord : ContentPartRecord {

        public virtual string CountryName { get; set; }
        public virtual int CountryId { get; set; }

        public virtual string ProvinceName { get; set; }
        public virtual int ProvinceId { get; set; }

        public virtual string CityName { get; set; }
        public virtual int CityId { get; set; }

        public virtual string AddressLine1 { get; set; }
        public virtual string AddressLine2 { get; set; }
        public virtual string PostalCode { get; set; }

        // Flag telling whether a pickup point had been selected for the
        // order this part is attached to.
        public virtual bool IsOrderPickupPoint { get; set; }
    }
}