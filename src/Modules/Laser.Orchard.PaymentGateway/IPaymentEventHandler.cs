using Orchard.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway {
    public interface IPaymentEventHandler : IEventHandler {
        void OnSuccess(int paymentId, int contentItemId);
        void OnError(int paymentId, int contentItemId);
    }
}