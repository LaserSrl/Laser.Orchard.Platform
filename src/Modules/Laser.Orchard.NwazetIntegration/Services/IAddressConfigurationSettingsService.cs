using Nwazet.Commerce.Models;
using Orchard;
using System.Collections.Generic;
using System.Globalization;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface IAddressConfigurationSettingsService : IDependency {
        TerritoryHierarchyPart GetConfiguredHierarchy(CultureInfo culture);
        TerritoryHierarchyPart GetConfiguredHierarchy(string culture = "");
        /// <summary>
        /// The Hierarchy to be used in address configuration. This is the
        /// ContentItem directly selected in configuration.
        /// </summary>
        TerritoryHierarchyPart ShippingCountriesHierarchy { get; }
        /// <summary>
        /// All valid hierarchies to be used in address configuration. There
        /// are cases where more than one hierarchy are considered in the 
        /// configuration, e.g. different localizations of a single hierarchy.
        /// </summary>
        IEnumerable<TerritoryHierarchyPart> ShippingCountriesHierarchies { get; }
        /// <summary>
        /// The Ids of all selected territories.
        /// </summary>
        int[] SelectedTerritoryIds { get; }
        /// <summary>
        /// The records corresponding to all selected territories.
        /// </summary>
        IEnumerable<TerritoryInternalRecord> SelectedTerritoryRecords { get; }
        /// <summary>
        /// The Ids of all territories selected to be used as Countries.
        /// </summary>
        int[] SelectedCountryIds { get; }
        /// <summary>
        /// The records corresponding to all selected Countries.
        /// </summary>
        IEnumerable<TerritoryInternalRecord> SelectedCountryTerritoryRecords { get; }
        /// <summary>
        /// The Ids of all territories selected to be used as Provinces.
        /// </summary>
        int[] SelectedProvinceIds { get; }
        /// <summary>
        /// The records corresponding to all selected Provinces.
        /// </summary>
        IEnumerable<TerritoryInternalRecord> SelectedProvinceTerritoryRecords { get; }
        /// <summary>
        /// The Ids of all territories selected to be used as Cities.
        /// </summary>
        int[] SelectedCityIds { get; }
        /// <summary>
        /// The records corresponding to all selected Cities.
        /// </summary>
        IEnumerable<TerritoryInternalRecord> SelectedCityTerritoryRecords { get; }
        ///// <summary>
        ///// All the configured Country ISO codes, with the Id corresponding to the
        ///// TerritoryInternalRecord for that country.
        ///// </summary>
        //IEnumerable<CountryAlpha2> CountryISOCodes { get; }
        ///// <summary>
        ///// Get the configured ISO Code for the territory with the given ID
        ///// </summary>
        ///// <param name="id">The Id of the territory for which we want the ISO code</param>
        ///// <returns></returns>
        //string GetCountryISOCode(int id);
    }
}
