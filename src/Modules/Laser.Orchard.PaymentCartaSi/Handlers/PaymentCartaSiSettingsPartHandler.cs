using Laser.Orchard.PaymentCartaSi.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentCartaSi.Handlers {
    public class PaymentCartaSiSettingsPartHandler : ContentHandler {

        public PaymentCartaSiSettingsPartHandler(IRepository<PaymentCartaSiSettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<PaymentCartaSiSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}