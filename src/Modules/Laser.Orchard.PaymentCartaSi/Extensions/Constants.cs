using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentCartaSi.Extensions {
    public class Constants {
        public const string PosName = "CartaSì X-Pay";
        public const string LocalArea = "Laser.Orchard.PaymentCartaSi";
    }

    public class EndPoints {
        public const string PaymentURL = "https://ecommerce.keyclient.it/ecomm/ecom/DispatcherServlet";
        public const string TestPaymentURL = "https://coll-ecommerce.keyclient.it/ecomm/ecomm/DispatcherServlet";
    }
}