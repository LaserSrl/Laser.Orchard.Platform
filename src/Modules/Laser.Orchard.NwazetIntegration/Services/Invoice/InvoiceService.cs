using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services.Invoice {
    public class InvoiceService : IInvoiceService{
        public IEnumerable<SelectListItem> BuildCustomerOptions(CustomerTypeOptions selected) {
            var options = new List<SelectListItem>();
            options.Add(new SelectListItem() {
                Value = CustomerTypeOptions.Individual.ToString(),
                Text = "Final consumer",
                Selected = selected == CustomerTypeOptions.Individual
            });
            options.Add(new SelectListItem() {
                Value = CustomerTypeOptions.LegalEntity.ToString(),
                Text = "Company or individual business",
                Selected = selected == CustomerTypeOptions.LegalEntity
            });

            return options;
        }
    }
}