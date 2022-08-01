using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.NwazetIntegration.Aspects {
    public interface IOrderExtensionAspect : IContent {
        /// <summary>
        /// Extend Order Creation using information from the Checkout context
        /// </summary>
        /// <param name="cvm"></param>
        void ExtendCreation(CheckoutViewModel cvm);
    }
}
