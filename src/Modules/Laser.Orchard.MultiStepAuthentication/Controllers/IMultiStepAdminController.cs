using Orchard;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.MultiStepAuthentication.Controllers {
    /// <summary>
    /// This interface is going to be implemented by those controllers dealing with the settings for
    /// the multi-step authentication implementations. By having them all respect an interface, it's
    /// possible to inject them to build the site navigation at runtime.
    /// </summary>
    public interface IMultiStepAdminController : IDependency {
        /// <summary>
        /// Name of the action to build the navigation link
        /// </summary>
        string ActionName { get; }
        /// <summary>
        /// Name of the controller to build the navigation link
        /// </summary>
        string ControllerName { get; }
        /// <summary>
        /// Name of the are to build the navigation link
        /// </summary>
        string AreaName { get; }
        /// <summary>
        /// Caption to use to build the navigation link
        /// </summary>
        LocalizedString Caption { get; }
    }
}
