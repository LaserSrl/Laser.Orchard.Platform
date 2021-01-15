using Laser.Orchard.StartupConfig.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IApiCredentialsManagementService : IDependency {
        /// <summary>
        /// returns the plain text secret for the user represented by the part.
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        string GetSecret(ApiCredentialsPart part);
    }
}
