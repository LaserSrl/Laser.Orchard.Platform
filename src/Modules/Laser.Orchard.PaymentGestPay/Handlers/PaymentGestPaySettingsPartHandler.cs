using Laser.Orchard.PaymentGestPay.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGestPay.Handlers {
    public class PaymentGestPaySettingsPartHandler : ContentHandler {

        public PaymentGestPaySettingsPartHandler(IRepository<PaymentGestPaySettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<PaymentGestPaySettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}