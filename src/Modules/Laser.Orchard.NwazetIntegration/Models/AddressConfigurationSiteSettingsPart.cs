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
        /// for the tenant, that will be signaled with an error notification until fixed.
        /// </summary>
        public int ShippingCountriesHierarchyId {
            get { return this.Retrieve(p => p.ShippingCountriesHierarchyId); }
            set { this.Store(p => p.ShippingCountriesHierarchyId, value); }
        }
        #endregion
        
    }
}
