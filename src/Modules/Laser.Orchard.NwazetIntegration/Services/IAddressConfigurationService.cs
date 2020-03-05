using Laser.Orchard.NwazetIntegration.Models;
using Nwazet.Commerce.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface IAddressConfigurationService : IDependency {
        /// <summary>
        /// Get the list of all the countries configured for the system.
        /// Implementations should be aware of proper content localization.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TerritoryPart> GetAllCountries();
        /// <summary>
        /// Get the list of all the countries configured for the system for a 
        /// given type of address.
        /// Implementations should be aware of proper content localization.
        /// </summary>
        /// <param name="addressRecordType">The type of address</param>
        /// <returns></returns>
        IEnumerable<TerritoryPart> GetAllCountries(AddressRecordType addressRecordType);
        /// <summary>
        /// Gets a list of options configured to be used in UI when selecting
        /// a country.
        /// </summary>
        /// <param name="id">Id of a country to preselect.</param>
        /// <returns></returns>
        List<SelectListItem> CountryOptions(int id = -1);
        /// <summary>
        /// Gets a list of options configured to be used in UI when selecting
        /// a country for a given typ fo address.
        /// </summary>
        /// <param name="addressRecordType">The type of address</param>
        /// <param name="id">Id of a country to preselect.</param>
        /// <returns></returns>
        List<SelectListItem> CountryOptions(AddressRecordType addressRecordType, int id = -1);
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
        /// Given the Name of a TerritoryInternalRecord (i.e. the territories' truth)
        /// find the corresponding configured country if it exists.
        /// </summary>
        /// <param name="name">The Name of a TerritoryInternalRecord for the country 
        /// we are looking for</param>
        /// <returns>The TerritoryPart for the desired country if found, null otherwise.</returns>
        /// <remarks>This method will not return the country the territory with the 
        /// given Name belongs to. It only returns a TerritoryPart if if matches the Name
        /// given and it is a country.</remarks>
        TerritoryPart GetCountry(string name);
        /// <summary>
        /// Get the list of all TerritoryParts that exist in the hierarchy underneath
        /// the given territory and that are also configured as cities.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        IEnumerable<TerritoryPart> GetAllCities(TerritoryPart parent);
        /// <summary>
        /// Get the list of all TerritoryParts that exist in the hierarchy underneath
        /// the given territory and that are also configured as cities and that are
        /// marked for the given address type (shipping/billing).
        /// </summary>
        /// <param name="addressRecordType">The type of address</param>
        /// <param name="parent"></param>
        /// <returns></returns>
        IEnumerable<TerritoryPart> GetAllCities(AddressRecordType addressRecordType, TerritoryPart parent);
        /// <summary>
        /// Given the Id of a TerritoryInternalRecord (i.e. the territories' truth)
        /// find the corresponding configured city if it exists.
        /// </summary>
        /// <param name="internalId">The Id of a TerritoryInternalRecord for the city 
        /// we are looking for</param>
        /// <returns>The TerritoryPart for the desired city if found, null otherwise.</returns>
        /// <remarks>This method will not return the city the territory with the 
        /// given ID belongs to. It only returns a TerritoryPart if if matches the Id
        /// given and it is a city.</remarks>
        TerritoryPart GetCity(int internalId);
        /// <summary>
        /// Given the Name of a TerritoryInternalRecord (i.e. the territories' truth)
        /// find the corresponding configured city if it exists.
        /// </summary>
        /// <param name="internalId">The Name of a TerritoryInternalRecord for the city 
        /// we are looking for</param>
        /// <returns>The TerritoryPart for the desired city if found, null otherwise.</returns>
        /// <remarks>This method will not return the city the territory with the 
        /// given Name belongs to. It only returns a TerritoryPart if if matches the Name
        /// given and it is a city.</remarks>
        TerritoryPart GetCity(string name);
        /// <summary>
        /// Get the list of all TerritoryParts that exist in the hierarchy underneath
        /// the given territory and that are also configured as provinces.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        IEnumerable<TerritoryPart> GetAllProvinces(TerritoryPart country);
        /// <summary>
        /// Get the list of all TerritoryParts that exist in the hierarchy underneath
        /// the given territory and that are also configured as provinces and that are
        /// marked for the given address type (shipping/billing).
        /// </summary>
        /// <param name="addressRecordType">The type of address</param>
        /// <param name="country"></param>
        /// <returns></returns>
        IEnumerable<TerritoryPart> GetAllProvinces(AddressRecordType addressRecordType, TerritoryPart country);
        /// <summary>
        /// Get the list of all TerritoryParts that exist in the hierarchy underneath
        /// the given territory, that are also configured as provinces, and that contain
        /// within the sub-hierarchy of their children the territory passed as city.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="city">The territory that we must have within the sub-hierarchy
        /// of the province territory.</param>
        /// <returns></returns>
        /// <remarks>We should expect this method to always return 0 results (no province found
        /// that matches the criteria) or 1 result (we found The province that matches the criteria).
        /// We are open to returning more results in some special cases, but the main reason
        /// why we are returning an IEnumerable is to simplify code using this API.</remarks>
        IEnumerable<TerritoryPart> GetAllProvinces(TerritoryPart country, TerritoryPart city);
        /// <summary>
        /// Get the list of all TerritoryParts that exist in the hierarchy underneath
        /// the given territory, that are also configured as provinces, and that contain
        /// within the sub-hierarchy of their children the territory passed as city and that are
        /// marked for the given address type (shipping/billing).
        /// </summary>
        /// <param name="addressRecordType">The type of address</param>
        /// <param name="country"></param>
        /// <param name="city">The territory that we must have within the sub-hierarchy
        /// of the province territory.</param>
        /// <returns></returns>
        /// <remarks>We should expect this method to always return 0 results (no province found
        /// that matches the criteria) or 1 result (we found The province that matches the criteria).
        /// We are open to returning more results in some special cases, but the main reason
        /// why we are returning an IEnumerable is to simplify code using this API.</remarks>
        IEnumerable<TerritoryPart> GetAllProvinces(AddressRecordType addressRecordType, TerritoryPart country, TerritoryPart city);
        /// <summary>
        /// Get a single Territory from the configured Hierarchy
        /// </summary>
        /// <param name="internalId">The Id of the internal record for the territory</param>
        /// <returns>The TerritoryPart, or null if it is not found.</returns>
        TerritoryPart SingleTerritory(int internalId);
        /// <summary>
        /// Get a single Territory from the configured Hierarchy
        /// </summary>
        /// <param name="name">The Name of the internal record for the territory</param>
        /// <returns>The TerritoryPart, or null if it is not found.</returns>
        TerritoryPart SingleTerritory(string name);
    }
}
