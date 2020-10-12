using Laser.Orchard.OpenAuthentication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.ViewModels {
    public class ProviderConfigurationViewModel {
        public int Id { get; set; }
        public int IsEnabled { get; set; }
        public string DisplayName { get; set; }
        public string ProviderName { get; set; }
        public string ProviderIdKey { get; set; }
        public string ProviderSecret { get; set; }
        public string ProviderIdentifier { get; set; }
        public string UserIdentifier { get; set; }
        public List<ProviderAttributeViewModel> Attributes { get; set; }
    }
}
