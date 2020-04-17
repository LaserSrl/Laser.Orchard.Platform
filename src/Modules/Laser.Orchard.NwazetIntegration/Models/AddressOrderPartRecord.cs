using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class AddressOrderPartRecord : ContentPartRecord {

        public virtual string ShippingCountryName { get; set; }
        public virtual int ShippingCountryId { get; set; }

        public virtual string ShippingCityName { get; set; }
        public virtual int ShippingCityId { get; set; }

        public virtual string ShippingProvinceName { get; set; }
        public virtual int ShippingProvinceId { get; set; }

        public virtual string BillingCountryName { get; set; }
        public virtual int BillingCountryId { get; set; }

        public virtual string BillingCityName { get; set; }
        public virtual int BillingCityId { get; set; }

        public virtual string BillingProvinceName { get; set; }
        public virtual int BillingProvinceId { get; set; }
    }
}