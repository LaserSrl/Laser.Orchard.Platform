using Laser.Orchard.PaymentGestPay.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGestPay.Services {
    public interface IGestPayTransactionServices : IDependency, IPosService {
        //string StartGestPayTransactionURL(int paymentId);
        TransactionOutcome ReceiveS2STransaction(string a, string b);
        string InterpretTransactionResult(string a, string b);
    }
}