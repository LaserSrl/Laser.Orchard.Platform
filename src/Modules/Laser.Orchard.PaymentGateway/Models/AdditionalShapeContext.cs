using Laser.Orchard.PaymentGateway.ViewModels;
using Orchard.DisplayManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Models {
    public class AdditionalShapeContext {
        public PaymentVM PaymentViewModel { get; set; }
        public dynamic ShapeFactory { get; set; }
    }
}