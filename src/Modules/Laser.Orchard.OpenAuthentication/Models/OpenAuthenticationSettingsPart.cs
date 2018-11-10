using Orchard.ContentManagement;

namespace Laser.Orchard.OpenAuthentication.Models {
    public class OpenAuthenticationSettingsPart : ContentPart<OpenAuthenticationSettingsPartRecord> {
        public bool AutoRegistrationEnabled {
            get { return this.Retrieve(x => x.AutoRegistrationEnabled); }
            set {this.Store(x => x.AutoRegistrationEnabled, value);}
        }
        public bool AutoMergeNewUsersEnabled {
            get { return this.Retrieve(x => x.AutoMergeNewUsersEnabled); }
            set { this.Store(x => x.AutoMergeNewUsersEnabled, value); }
        }
        public string AppDirectBaseUrl {
            get { return this.Retrieve(x => x.AppDirectBaseUrl); }
            set { this.Store(x => x.AppDirectBaseUrl, value); }
        }
    }
}