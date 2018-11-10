using Laser.Orchard.MultiStepAuthentication.Models;
using Orchard;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.MultiStepAuthentication.Services {
    /// <summary>
    /// Base interface for OTPServices. In general, implementations of derived interfaces should be used, 
    /// because they will be more specific. See for example INonceService.
    /// </summary>
    public interface IBaseOTPService : IDependency {
        
        /// <summary>
        /// Compares the information from the context with the information form the records
        /// to decide whether the password provided in the context is valid.
        /// </summary>
        /// <param name="context">The object containning the information we are trying to validate.</param>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the context is null.</exception>
        /// <exception cref="ArgumentException">Throws an ArgumentException if either the password or the
        /// user in the context are null.</exception>
        /// <returns>True if the information from the context is valid, false otherwise.</returns>
        bool ValidatePassword(OTPContext context);

        /// <summary>
        /// Generate a new one-time password for the user.
        /// </summary>
        /// <param name="user">The user for whom the new password will be generated.</param>
        /// <returns>The generated password.</returns>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the user is null.</exception>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the user is not valid.</exception>
        string GenerateOTP(IUser user);

        /// <summary>
        /// Generate a new one-time password for the user using the information provided.
        /// </summary>
        /// <param name="user">The user for whom the new password will be generated.</param>
        /// <param name="additionalInformation">Additional information that may be used to generate
        /// the password.</param>
        /// <returns>The generated password.</returns>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the user is null.</exception>
        /// <exception cref="ArgumentException">Throws an ArgumentException the user is not valid.</exception>
        /// <exception cref="ArgumentNullException">Implementations may throw an ArgumentNullException if 
        /// the additionalInformation is null.</exception>
        /// <exception cref="ArgumentException">Implmentations may throw an ArgumentException if validation
        /// of additionalInformation fails.</exception>
        string GenerateOTP(IUser user, Dictionary<string, string> additionalInformation);

        /// <summary>
        /// Attempts to send the one-time password provided to the corresponding user.
        /// </summary>
        /// <param name="otp">The record for the one-time password.</param>
        /// <param name="channel">The channel type used for the delivery. If null, the service will attempt
        /// to send the password through all channel types.</param>
        /// <returns>True if the password was sent succesfully, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the record is null.</exception>
        bool SendOTP(OTPRecord otp, DeliveryChannelType? channel);





        /// <summary>
        /// Attempts to send a new one-time password to the user provided.
        /// </summary>
        /// <param name="user">The user for whom a new one-time password will be generated.</param>
        /// <param name="channel">The channel type used for the delivery. If null, the service will attempt
        /// to send the password through all channel types.</param>
        /// <returns>True if the password was generated and sent succesfully, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the user is null.</exception>
        /// <exception cref="ArgumentException">Throws an ArgumentException the user is not valid.</exception>
        bool SendNewOTP(IUser user, DeliveryChannelType? channel);

        /// <summary>
        /// Attempts to send a new one-time password to the user provided.
        /// </summary>
        /// <param name="user">The user for whom a new one-time password will be generated.</param>
        /// <param name="additionalInformation">Additional information that may be used to generate
        /// the password.</param>
        /// <param name="channel">The channel type used for the delivery. If null, the service will attempt
        /// to send the password through all channel types.</param>
        /// <returns>True if the password was generated and sent succesfully, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the user is null.</exception>
        /// <exception cref="ArgumentException">Throws an ArgumentException the user is not valid.</exception>
        /// <exception cref="ArgumentNullException">Implementations may throw an ArgumentNullException if 
        /// the additionalInformation is null.</exception>
        /// <exception cref="ArgumentException">Implmentations may throw an ArgumentException if validation
        /// of additionalInformation fails.</exception>
        bool SendNewOTP(IUser user, Dictionary<string, string> additionalInformation, DeliveryChannelType? channel);

        /// <summary>
        /// Attempts to send a new one-time password to the user provided.
        /// </summary>
        /// <param name="user">The user for whom a new one-time password will be generated.</param>
        /// <param name="additionalInformation">Additional information that may be used to generate
        /// the password.</param>
        /// <param name="channel">The channel type used for the delivery. If null, the service will attempt
        /// to send the password through all channel types.</param>
        /// <param name="flow">The program flow to use, same data required by different channel (web/app) must generate a nonce's link or a link to download special nonce with custom structure used for app interaction </param>
        /// <returns></returns>
        bool SendNewOTP(IUser user, Dictionary<string, string> additionalInformation, DeliveryChannelType? channel,FlowType? flow);
       
    }
}
