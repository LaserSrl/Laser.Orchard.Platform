using Nwazet.Commerce.Models;
using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface IAddressConfigurationSettingsService : IDependency {
        TerritoryHierarchyPart ShippingCountriesHierarchy { get; }
        IEnumerable<TerritoryHierarchyPart> ShippingCountriesHierarchies { get; }
        string[] SelectedTerritoryNames { get; }
        IEnumerable<TerritoryInternalRecord> SelectedTerritoryRecords { get; }
    }
}
