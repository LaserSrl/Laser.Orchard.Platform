using Contrib.Profile.Services;
using Orchard.ContentManagement.MetaData;

namespace Laser.Orchard.UsersExtensions.Services {
    public class DefaultFrontEndSettingsProvider : DefaultFrontEndSettingsProviderBase {
        public DefaultFrontEndSettingsProvider(
            IContentDefinitionManager contentDefinitionManager)
                : base("UserRegistrationPolicyPart", false, true, contentDefinitionManager) {}
    }
}