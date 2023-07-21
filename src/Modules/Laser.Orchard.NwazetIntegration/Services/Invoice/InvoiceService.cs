using Orchard.Localization;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services.Invoice {
    public class InvoiceService : IInvoiceService {

        public InvoiceService() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<SelectListItem> BuildCustomerOptions(CustomerTypeOptions selected) {
            var options = new List<SelectListItem>();
            options.Add(new SelectListItem() {
                Value = CustomerTypeOptions.Individual.ToString(),
                Text = T("Final consumer").Text,
                Selected = selected == CustomerTypeOptions.Individual
            });
            options.Add(new SelectListItem() {
                Value = CustomerTypeOptions.LegalEntity.ToString(),
                Text = T("Company or individual business").Text,
                Selected = selected == CustomerTypeOptions.LegalEntity
            });

            return options;
        }
    }
}