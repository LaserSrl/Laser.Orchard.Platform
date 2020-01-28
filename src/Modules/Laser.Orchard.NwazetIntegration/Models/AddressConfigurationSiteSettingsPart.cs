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
        /// <summary>
        /// Reset all detail configuration
        /// </summary>
        public void ResetDetails() {
            // TODO: clear all detail configuration
        }
        public string SerializedSelectedTerritories {
            get { return this.Retrieve(p => p.SerializedSelectedTerritories); }
            set { this.Store(p => p.SerializedSelectedTerritories, value); }
        }

        public int[] SelectedTerritories {
            get {
                return string.IsNullOrWhiteSpace(SerializedSelectedTerritories)
                    ? new int[] { }
                    : JsonConvert.DeserializeObject<int[]>(SerializedSelectedTerritories);
            }
        }

        //public int[] SelectedCountries {

        //}
        //public int[] SelectedProvinces {

        //}
        //public int[] SelectedCities {

        //}
        #endregion
    }
}
