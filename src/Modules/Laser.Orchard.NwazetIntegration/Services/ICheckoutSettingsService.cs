using Orchard;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.NwazetIntegration.Services {
    /// <summary>
    /// Centralize access to checkout settings
    /// </summary>
    public interface ICheckoutSettingsService : IDependency{
        bool AuthenticationRequired { get; }
        bool UserMayCheckout(IUser user = null);
    }
}
