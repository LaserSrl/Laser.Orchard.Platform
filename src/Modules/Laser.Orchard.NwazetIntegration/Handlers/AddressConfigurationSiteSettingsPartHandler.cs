using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class AddressConfigurationSiteSettingsPartHandler : ContentHandler {
        private readonly ISignals _signals;
        private readonly IAddressConfigurationSettingsService _addressConfigurationSettingsService;
        public AddressConfigurationSiteSettingsPartHandler(
            ISignals signals,
            IAddressConfigurationSettingsService addressConfigurationSettingsService) {

            _signals = signals;
            _addressConfigurationSettingsService = addressConfigurationSettingsService;

            Filters.Add(new ActivatingFilter<AddressConfigurationSiteSettingsPart>("Site"));

            // Evict cached content when updated, removed or destroyed.
            OnUpdated<AddressConfigurationSiteSettingsPart>(
                (context, part) => Invalidate());
            OnImported<AddressConfigurationSiteSettingsPart>(
                (context, part) => Invalidate());
            OnPublished<AddressConfigurationSiteSettingsPart>(
                (context, part) => Invalidate());
            OnRemoved<AddressConfigurationSiteSettingsPart>(
                (context, part) => Invalidate());
            OnDestroyed<AddressConfigurationSiteSettingsPart>(
                (context, part) => Invalidate());
            // Also invalidate when the Hierarchy part has been updated
            OnUpdated<TerritoryHierarchyPart>(
                (context, part) => Invalidate(part));
            OnImported<TerritoryHierarchyPart>(
                (context, part) => Invalidate(part));
            OnPublished<TerritoryHierarchyPart>(
                (context, part) => Invalidate(part));
            OnRemoved<TerritoryHierarchyPart>(
                (context, part) => Invalidate(part));
            OnDestroyed<TerritoryHierarchyPart>(
                (context, part) => Invalidate(part));
            // Also invalidate when one of the selected territories has been updated
            OnUpdated<TerritoryAdministrativeTypePart>(
                (context, part) => Invalidate(part));
            OnImported<TerritoryAdministrativeTypePart>(
                (context, part) => Invalidate(part));
            OnPublished<TerritoryAdministrativeTypePart>(
                (context, part) => Invalidate(part));
            OnRemoved<TerritoryAdministrativeTypePart>(
                (context, part) => Invalidate(part));
            OnDestroyed<TerritoryAdministrativeTypePart>(
                (context, part) => Invalidate(part));
            // Also invalidate when we update whether the territory is for shipping or billing
            OnUpdated<TerritoryAddressTypePart>(
                (context, part) => Invalidate(part));
            OnImported<TerritoryAddressTypePart>(
                (context, part) => Invalidate(part));
            OnPublished<TerritoryAddressTypePart>(
                (context, part) => Invalidate(part));
            OnRemoved<TerritoryAddressTypePart>(
                (context, part) => Invalidate(part));
            OnDestroyed<TerritoryAddressTypePart>(
                (context, part) => Invalidate(part));
        }

        private void Invalidate(TerritoryHierarchyPart part) {
            // if the hierarchy is the one selected or one of its localizations
            // we invalidate the cache
            if (!_addressConfigurationSettingsService.ShippingCountriesHierarchies.Any()
                || _addressConfigurationSettingsService.ShippingCountriesHierarchies
                .Select(thp => thp.Id)
                .Contains(part.Id)) {
                Invalidate();
            }
        }

        private void Invalidate(TerritoryAddressTypePart part) {
            var territory = part.As<TerritoryPart>();
            Invalidate(territory);
        }

        private void Invalidate(TerritoryAdministrativeTypePart part) {
            var territory = part.As<TerritoryPart>();
            Invalidate(territory);
        }

        private void Invalidate(TerritoryPart territory) {
            if (territory != null) {
                var hId = territory.Hierarchy?.Id;
                // if the territory belongs the selected hierarchy or one
                // of its localizations
                var localizations = _addressConfigurationSettingsService.ShippingCountriesHierarchies;
                var hierarchyOk = hId.HasValue && localizations.Any()
                    && localizations.Select(thp => thp.Id).Contains(hId.Value);
                if (hierarchyOk) {
                    Invalidate();
                }
            }

        }

        private void Invalidate() {
            _signals.Trigger(Constants.CacheEvictSignal);
        }
    }
}
