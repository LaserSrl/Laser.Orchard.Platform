using Contrib.Profile.Services;
using Orchard.ContentManagement.MetaData;

namespace Laser.Orchard.StartupConfig.Services {
    public class FavoriteCultureDefaultFrontEndSettingsProvider : DefaultFrontEndSettingsProviderBase {
        public FavoriteCultureDefaultFrontEndSettingsProvider(
            IContentDefinitionManager contentDefinitionManager)
                : base("FavoriteCulturePart", true, true, contentDefinitionManager) { }
    }
}