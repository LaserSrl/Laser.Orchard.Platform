using Laser.Orchard.Cookies;
using Orchard.ContentManagement;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class EcommerceInvoiceSettingsPart : ContentPart {
        public bool EnableInvoiceRequest {
            get { return this.Retrieve(x => x.EnableInvoiceRequest); }
            set { this.Store(x => x.EnableInvoiceRequest, value); }
        }

        public bool InvoiceRequestDefaultValue {
            get { return this.Retrieve(x => x.InvoiceRequestDefaultValue); }
            set { this.Store(x => x.InvoiceRequestDefaultValue, value); }
        }

        public bool InvoiceRequestForceChoice {
            get { return this.Retrieve(x => x.InvoiceRequestForceChoice); }
            set { this.Store(x => x.InvoiceRequestForceChoice, value); }
        }

    }
}