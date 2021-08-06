using Orchard.Environment.Extensions;
using System;

namespace Laser.Orchard.StartupConfig.Security {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public sealed class BearerTokenAuthenticationTicket {

        public BearerTokenAuthenticationTicket(
            int version, string name,
            DateTime expiration, DateTime issueDate,
            string userData) {

            Version = version;
            Name = name;
            Expiration = expiration;
            IssueDate = issueDate;
            Expired = Expiration < DateTime.Now;
            UserData = userData;
        }

        /// <summary>
        /// Get the version number of the ticket.
        /// </summary>
        public int Version { get; private set; }
        /// <summary>
        /// Get the username associated with the ticket.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Get the date and time at which the ticket expires.
        /// </summary>
        public DateTime Expiration { get; private set; }
        /// <summary>
        /// Get the date and time when the ticket was issued.
        /// </summary>
        public DateTime IssueDate { get; }
        /// <summary>
        /// Get a value indicating whether the ticket has expired.
        /// </summary>
        public bool Expired { get; }
        /// <summary>
        /// Get the serialized dictionary representing the user.
        /// </summary>
        public string UserData { get; }
    }
}