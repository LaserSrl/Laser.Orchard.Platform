using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Security;

namespace Laser.Orchard.StartupConfig.Security.Providers {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class TenantNameBearerTokenDataProvider : BaseBearerTokenDataProvider {
        private readonly ShellSettings _settings;

        public TenantNameBearerTokenDataProvider(
            ShellSettings settings) : base(false) {

            _settings = settings;
        }

        public override string Key {
            get { return "TenantName"; }
        }

        protected override string Value(IUser user) {
            return _settings.Name;
        }
    }
}