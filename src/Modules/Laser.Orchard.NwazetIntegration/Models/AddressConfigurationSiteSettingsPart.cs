using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class AddressConfigurationSiteSettingsPart : ContentPart {

        #region Base configuration
        /// <summary>
        /// This value being 0 means no hierarchy has been selected. That is a configuration error
        /// for the teneat, that will be signaled with an error notification until fixed.
        /// </summary>
        public int ShippingCountriesHierarchyId {
            get { return this.Retrieve(p => p.ShippingCountriesHierarchyId); }
            set { this.Store(p => p.ShippingCountriesHierarchyId, value); }
        }
        #endregion

        #region Details configuration
        // Detailed configuration is now doen with Parts attached to territories
        // so we are more flexible/extensible with the features we can build with
        // the system

        ///// <summary>
        ///// Reset all detail configuration
        ///// </summary>
        //public void ResetDetails() {
        //    // TODO: clear all detail configuration
        //    SerializedSelectedCountries = "[]";
        //    SerializedSelectedProvinces = "[]";
        //    SerializedSelectedCities = "[]";
        //}

        //public string SerializedSelectedCountries {
        //    get { return this.Retrieve(p => p.SerializedSelectedCountries); }
        //    set { this.Store(p => p.SerializedSelectedCountries, value); }
        //}
        //public int[] SelectedCountries {
        //    get {
        //        return string.IsNullOrWhiteSpace(SerializedSelectedCountries)
        //            ? new int[] { }
        //            : JsonConvert.DeserializeObject<int[]>(SerializedSelectedCountries);
        //    }
        //}
        //public string SerializedSelectedProvinces {
        //    get { return this.Retrieve(p => p.SerializedSelectedProvinces); }
        //    set { this.Store(p => p.SerializedSelectedProvinces, value); }
        //}
        //public int[] SelectedProvinces {
        //    get {
        //        return string.IsNullOrWhiteSpace(SerializedSelectedProvinces)
        //            ? new int[] { }
        //            : JsonConvert.DeserializeObject<int[]>(SerializedSelectedProvinces);
        //    }
        //}
        //public string SerializedSelectedCities {
        //    get { return this.Retrieve(p => p.SerializedSelectedCities); }
        //    set { this.Store(p => p.SerializedSelectedCities, value); }
        //}
        //public int[] SelectedCities {
        //    get {
        //        return string.IsNullOrWhiteSpace(SerializedSelectedCities)
        //            ? new int[] { }
        //            : JsonConvert.DeserializeObject<int[]>(SerializedSelectedCities);
        //    }
        //}

        //public string SerializedCountryCodes {
        //    get { return this.Retrieve(p => p.SerializedCountryCodes); }
        //    set { this.Store(p => p.SerializedCountryCodes, value); }
        //}
        //public CountryAlpha2[] CountryCodes {
        //    get {
        //        return string.IsNullOrWhiteSpace(SerializedCountryCodes)
        //            ? new CountryAlpha2[] { }
        //            : JsonConvert.DeserializeObject<CountryAlpha2[]>(SerializedCountryCodes);
        //    }
        //}

        #endregion
    }
}
