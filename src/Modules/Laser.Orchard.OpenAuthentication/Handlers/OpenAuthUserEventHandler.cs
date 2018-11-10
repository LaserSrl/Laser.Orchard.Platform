using Laser.Orchard.OpenAuthentication.Events;

namespace Laser.Orchard.OpenAuthentication.Handlers {
    /// <summary>
    /// Empty implementation of the interface, used only for dependency resolution.
    /// </summary>
    public class OpenAuthUserEventHandler : IOpenAuthUserEventHandler {
        public void Created(CreatedOpenAuthUserContext context) { }

        public void Creating(CreatingOpenAuthUserContext context) { }

        public void ProviderRecordCreated(CreatedOpenAuthUserContext context) { }

        public void ProviderRecordUpdated(CreatedOpenAuthUserContext context) { }
    }
}