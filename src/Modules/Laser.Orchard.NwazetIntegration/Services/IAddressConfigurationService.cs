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
        /// <summary>
        /// Given the Id of a TerritoryInternalRecord (i.e. the territories' truth)
        /// find the corresponding configured country if it exists.
        /// </summary>
        /// <param name="internalId">The Id of a TerritoryInternalRecord for the country 
        /// we are looking for</param>
        /// <returns>The TerritoryPart for the desired country if found, null otherwise.</returns>
        /// <remarks>This method will not return the country the territory with the 
        /// given ID belongs to. It only returns a TerritoryPart if if matches the Id
        /// given and it is a country.</remarks>
        TerritoryPart GetCountry(int internalId);
        /// <summary>
        /// Get the list of all TerritoryParts that exist in the hierarchy underneath
        /// the given territory and that are also configured as cities.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        IEnumerable<TerritoryPart> GetAllCities(TerritoryPart parent);
    }
}
