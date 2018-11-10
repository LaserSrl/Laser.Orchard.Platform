using Orchard.ContentManagement;

namespace Laser.Orchard.StartupConfig.Models {
    public class EnvironmentVariablesSettingsPart : ContentPart {
            public string FallbackTenants {
            get { return this.Retrieve(x => x.FallbackTenants); }
            set { this.Store(x => x.FallbackTenants, value); }
        }
    }
}