using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using System.Collections.Generic;
using Laser.Orchard.CulturePicker.Models;
using Orchard;

namespace Laser.Orchard.CulturePicker.Services {
    public interface ILocalizableContentService : IDependency {
        bool TryFindLocalizedRoute(ContentItem routableContent, string cultureName, out AutoroutePart localizedRoute);
        bool TryGetRouteForUrl(string url, out AutoroutePart route);
        IList<ExtendedCultureRecord> AvailableTranslations(string url, bool isHomePage=false);

        bool TryGetLocalizedId(int originalId, string cultureName, out int translatedIds);
        bool TryGetLocalizedId(IList<int> originalIds, string cultureName, out IList<int> translatedIds);
    }
}