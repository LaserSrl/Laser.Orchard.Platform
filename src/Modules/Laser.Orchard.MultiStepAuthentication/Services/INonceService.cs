using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.MultiStepAuthentication.Services {
    /// <summary>
    /// Interface for IOTPServices that produce a nonce.
    /// </summary>
    public interface INonceService : IBaseOTPService {

        /// <summary>
        /// Validates the nonce and returns the IUser corresponding to it.
        /// </summary>
        /// <param name="nonce">The nonce to validate.</param>
        /// <returns>The IUser corresponding to the nonce used, or null if no corresponding
        /// user was found.</returns>
        IUser UserFromNonce(string nonce);

        /// <summary>
        /// Validates the nonce and returns the IUser corresponding to it.
        /// </summary>
        /// <param name="nonce">The nonce to validate.</param>
        /// <param name="additionalInformation">Further additional information used.</param>
        /// <returns>The IUser corresponding to the nonce used, or null if no corresponding
        /// user was found.</returns>
        IUser UserFromNonce(string nonce, Dictionary<string, string> additionalInformation);
    }
}
