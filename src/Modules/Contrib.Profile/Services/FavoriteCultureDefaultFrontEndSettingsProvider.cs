using Orchard.ContentManagement.MetaData;

namespace Contrib.Profile.Services {
    public class FavoriteCultureDefaultFrontEndSettingsProvider : DefaultFrontEndSettingsProviderBase {
        public FavoriteCultureDefaultFrontEndSettingsProvider(
            IContentDefinitionManager contentDefinitionManager)
                : base("FavoriteCulturePart", true, true, contentDefinitionManager) { }
    }
}