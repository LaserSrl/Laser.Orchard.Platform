using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.CulturePicker.Services {
    public interface ILocalizableRouteService : IDependency {
        /// <summary>
        /// Defines the order (ascendent) of the execution of the implementations. 
        /// </summary>
        int Priority { get; }
        bool TryFindLocalizedUrl(LocalizableRouteContext localizableRouteContext);
    }
}
