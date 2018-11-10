using Laser.Orchard.MultiStepAuthentication.Models;
using Orchard;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.MultiStepAuthentication.Services {
    public interface IOTPRepositoryService : IDependency {
        /// <summary>
        /// Get all OTPs for a given user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="OTPType">Optional: the type assigned to the OTPs we want.</param>
        /// <returns>All OTPs found for the user.</returns>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the user is null.</exception>  
        IEnumerable<OTPRecord> Get(IUser user, string OTPType = null);

        /// <summary>
        /// Get an OTPRecord with the corresponding password.
        /// </summary>
        /// <param name="password">The password to search</param>
        /// <param name="OTPType">Optional: Limit the search to OTPs of the given type.</param>
        /// <returns>The OTP found, or null if none are found</returns>
        OTPRecord Get(string password, string OTPType = null);

        /// <summary>
        /// Adds the OTPRecord specified.
        /// </summary>
        /// <param name="otp">The record to add.</param>
        /// <returns>The added OTPRecord.</returns>
        OTPRecord AddOTP(OTPRecord otp);

        /// <summary>
        /// Deletes the given OTPRecord.
        /// </summary>
        /// <param name="otp">The OTPRecord to delete.</param>
        void Delete(OTPRecord otp);

        /// <summary>
        /// Deletes all OTPRecords for the given user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="OTPType">Optional: Limit the search to OTPs of the given type.</param>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the user is null.</exception> 
        void Delete(IUser user, string OTPType = null);

        /// <summary>
        /// Deletes all expired OTPs.
        /// </summary>
        /// <param name="OTPType">Optional: Limit the search to OTPs of the given type.</param>
        void DeleteExpired(string OTPType = null);

        /// <summary>
        /// Deletes all expired OTPs for the given user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="OTPType">Optional: Limit the search to OTPs of the given type.</param>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the user is null.</exception> 
        void DeleteExpired(IUser user, string OTPType = null);

    }
}
