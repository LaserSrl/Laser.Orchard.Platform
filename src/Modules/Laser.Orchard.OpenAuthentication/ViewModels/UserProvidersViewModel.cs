using System.Collections.Generic;
using Laser.Orchard.OpenAuthentication.Models;

namespace Laser.Orchard.OpenAuthentication.ViewModels {
    public class UserProvidersViewModel {
        public IList<UserProviderEntry> Providers { get; set; }
    }
}