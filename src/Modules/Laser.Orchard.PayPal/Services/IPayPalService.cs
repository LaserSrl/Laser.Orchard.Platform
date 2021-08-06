using Laser.Orchard.PayPal.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PayPal.Services {
    public interface IPayPalService : IDependency {
        CheckOrderResult VerifyOrderIdPayPal(string oid);
    }
}