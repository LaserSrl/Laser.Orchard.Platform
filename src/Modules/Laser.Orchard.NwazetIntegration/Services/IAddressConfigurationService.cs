using Nwazet.Commerce.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface IAddressConfigurationService : IDependency {
        /// <summary>
        /// Get the list of all the countries configured for the system.
        /// Implementations should be aware of proper content localization.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TerritoryPart> GetAllCountries();
    }
}
