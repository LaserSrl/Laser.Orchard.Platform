using Orchard;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentCartaSi.Services {
    public interface ICartaSiTransactionService : IDependency, IPosService {
        string StartCartaSiTransactionURL(int paymentId);
        string ReceiveUndo(string importo, string divisa, string codTrans, string esito);
        string HandleOutcomeTransaction(NameValueCollection qs);
        string HandleS2STransaction(NameValueCollection qs);

    }
}