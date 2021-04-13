using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface ICheckoutExtensionProvider : IDependency {
        /// <summary>
        /// Shapes meant to be injected in the shape for the form that
        /// starts the checkout process.
        /// </summary>
        /// <returns></returns>
        IEnumerable<dynamic> AdditionalCheckoutStartShapes();

        /// <summary>
        /// This will validate the values received when posting to start
        /// the checkout process.
        /// </summary>
        /// <param name="context"></param>
        void ProcessAdditionalCheckoutStartInformation(CheckoutExtensionContext context);
    }
}
