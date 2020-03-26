using Nwazet.Commerce.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface IGTMMeasuringPurchaseService : IDependency {
        decimal GetVatDue(OrderPart orderPart);
    }
}