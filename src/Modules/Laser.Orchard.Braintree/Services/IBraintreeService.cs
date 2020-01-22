using Laser.Orchard.Braintree.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bt = Braintree;

namespace Laser.Orchard.Braintree.Services
{
    public interface IBraintreeService : IDependency
    {
        string GetClientToken();
        TransactionResult Pay(string paymentMethodNonce, decimal amount, Dictionary<string, string> customFields);
        TransactionResult Pay(PaymentContext context);
    }
}