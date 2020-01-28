using Nwazet.Commerce.Models;
using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface IAddressConfigurationSettingsService : IDependency {
        TerritoryHierarchyPart ShippingCountriesHierarchy { get; }
        IEnumerable<TerritoryHierarchyPart> ShippingCountriesHierarchies { get; }
        int[] SelectedTerritoryIds { get; }
        IEnumerable<TerritoryInternalRecord> SelectedTerritoryRecords { get; }
    }
}
