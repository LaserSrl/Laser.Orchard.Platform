using System.Collections.Generic;
using System.Text.RegularExpressions;
using Orchard.Events;
using Orchard.Security;
using Orchard.Users.Models;

namespace Laser.Orchard.OpenAuthentication.Events {
    public interface IOpenAuthUserEventHandler : IEventHandler {
        /// <summary>
        /// Called before a User is created
        /// </summary>
        void Creating(CreatingOpenAuthUserContext context);

        /// <summary>
        /// Called after a user has been created
        /// </summary>
        void Created(CreatedOpenAuthUserContext context);

        void ProviderRecordCreated(CreatedOpenAuthUserContext context);

        void ProviderRecordUpdated(CreatedOpenAuthUserContext context);
    }
}