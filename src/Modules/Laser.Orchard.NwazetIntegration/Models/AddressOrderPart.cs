using Nwazet.Commerce.Aspects;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class AddressOrderPart 
        : ContentPart<AddressOrderPartRecord>, ITerritoryAddressAspect {

        public string ShippingCountryName {
            get { return Retrieve(r => r.ShippingCountryName); }
            set { Store(r => r.ShippingCountryName, value); }
        }
        public int ShippingCountryId {
            get { return Retrieve(r => r.ShippingCountryId); }
            set { Store(r => r.ShippingCountryId, value); }
        }
        public string ShippingCityName {
            get { return Retrieve(r => r.ShippingCityName); }
            set { Store(r => r.ShippingCityName, value); }
        }
        public int ShippingCityId {
            get { return Retrieve(r => r.ShippingCityId); }
            set { Store(r => r.ShippingCityId, value); }
        }
        public string ShippingProvinceName {
            get { return Retrieve(r => r.ShippingProvinceName); }
            set { Store(r => r.ShippingProvinceName, value); }
        }
        public int ShippingProvinceId {
            get { return Retrieve(r => r.ShippingProvinceId); }
            set { Store(r => r.ShippingProvinceId, value); }
        }

        public string BillingCountryName {
            get { return Retrieve(r => r.BillingCountryName); }
            set { Store(r => r.BillingCountryName, value); }
        }
        public int BillingCountryId {
            get { return Retrieve(r => r.BillingCountryId); }
            set { Store(r => r.BillingCountryId, value); }
        }
        public string BillingCityName {
            get { return Retrieve(r => r.BillingCityName); }
            set { Store(r => r.BillingCityName, value); }
        }
        public int BillingCityId {
            get { return Retrieve(r => r.BillingCityId); }
            set { Store(r => r.BillingCityId, value); }
        }
        public string BillingProvinceName {
            get { return Retrieve(r => r.BillingProvinceName); }
            set { Store(r => r.BillingProvinceName, value); }
        }
        public int BillingProvinceId {
            get { return Retrieve(r => r.BillingProvinceId); }
            set { Store(r => r.BillingProvinceId, value); }
        }

        public IEnumerable<int> TerritoriesIds => 
            new int[] { ShippingCityId, ShippingProvinceId, ShippingCountryId,
                BillingCityId, BillingProvinceId, BillingCountryId };

        public int CountryId => ShippingCountryId;

        public int ProvinceId => ShippingProvinceId;

        public int CityId => ShippingCityId;
    }
}