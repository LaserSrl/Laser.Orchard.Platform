using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services.Invoice {
    public interface IInvoiceService : IDependency {
        IEnumerable<SelectListItem> BuildCustomerOptions(CustomerTypeOptions selected);
    }
}
