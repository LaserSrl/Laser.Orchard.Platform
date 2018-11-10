using Laser.Orchard.Policy.ViewModels;
using Orchard.Events;

namespace Laser.Orchard.Policy.Events {
    public interface IPolicyEventHandler: IEventHandler {
        void PolicyChanged(PolicyEventViewModel policyData);
    }
}