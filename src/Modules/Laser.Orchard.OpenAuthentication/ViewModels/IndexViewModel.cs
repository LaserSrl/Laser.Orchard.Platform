using System.Collections.Generic;
using Laser.Orchard.OpenAuthentication.Models;

namespace Laser.Orchard.OpenAuthentication.ViewModels {
    public class IndexViewModel {
        public bool AutoRegistrationEnabled { get; set; }
        public bool AutoMergeNewUsersEnabled { get; set; }

        public IEnumerable<ProviderConfigurationViewModel> CurrentProviders { get; set; }
        public string AppDirectBaseUrl { get; set; }
        public bool ShowAppDirectSetting { get; set; }
    }
}