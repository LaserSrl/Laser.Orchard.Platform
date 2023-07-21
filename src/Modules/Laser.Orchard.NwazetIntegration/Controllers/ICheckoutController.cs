using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.NwazetIntegration.Controllers {
    public interface ICheckoutController {
        /// <summary>
        /// This method helps validation by providers extending checkout.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        bool TryUpdateModel<TModel>(TModel model) where TModel : class;
    }
}
