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
    /// Describes an interface for services responsible for sensing a one-time password (or nonce)
    /// to users.
    /// </summary>
    public interface IOTPDeliveryService : IDependency {
        /// <summary>
        /// Sends the password or nonce to the user.
        /// </summary>
        /// <param name="otp">The record containing the information about hte password or nonce to be delivered.</param>
        /// <param name="user">The user we should send the password to.</param>
        /// <returns>True if the delivery happened successfully, false otherwise.</returns>
        /// <remarks>Validation errors in this method should be handled internally and only cause the method to
        /// return false.</remarks>
        bool TrySendOTP(OTPRecord otp, IUser user);

        /// <summary>
        /// Sends the password or nonce to the user.
        /// </summary>
        /// <param name="otp">The record containing the information about hte password or nonce to be delivered.</param>
        /// <param name="user">The user we should send the password to.</param>
        /// <returns>True if the delivery happened successfully, false otherwise.</returns>
        /// <remarks>Validation errors in this method should be handled internally and only cause the method to
        /// return false.</remarks>
        bool TrySendOTP(OTPRecord otp, IUser user,FlowType? flow);

        /// <summary>
        /// Tells the channel type used by this delivery service. It allows to prevent sending several communication
        /// on the same channel for a single authentication attempt.
        /// </summary>
        DeliveryChannelType ChannelType { get; set; }

        /// <summary>
        /// Used for sorting delivery services.
        /// </summary>
        int Priority { get; set; }
    }
}
