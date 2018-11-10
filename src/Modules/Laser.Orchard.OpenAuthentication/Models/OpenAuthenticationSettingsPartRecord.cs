using Orchard.ContentManagement.Records;

namespace Laser.Orchard.OpenAuthentication.Models {
    public class OpenAuthenticationSettingsPartRecord : ContentPartRecord {
        public virtual bool AutoRegistrationEnabled { get; set; }
        public virtual bool AutoMergeNewUsersEnabled { get; set; }
        public virtual string AppDirectBaseUrl { get; set; }
       
    }
}