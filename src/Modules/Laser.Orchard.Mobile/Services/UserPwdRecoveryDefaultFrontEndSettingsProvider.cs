using Contrib.Profile.Services;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile.Services {
    [OrchardFeature("Laser.Orchard.Sms")]
    public class UserPwdRecoveryDefaultFrontEndSettingsProvider : DefaultFrontEndSettingsProviderBase {
        public UserPwdRecoveryDefaultFrontEndSettingsProvider(
            IContentDefinitionManager contentDefinitionManager)
                : base("UserPwdRecoveryPart", false, true, contentDefinitionManager) { }
    }
}