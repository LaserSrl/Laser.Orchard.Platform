using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ViewModels {
    public class ApiCredentialsPartViewModel {
        public string Key { get; set; }
        public string Secret { get; set; }

        public bool HasCredentials() {
            return !string.IsNullOrWhiteSpace(Key)
                && !string.IsNullOrWhiteSpace(Secret);
        }
    }
}