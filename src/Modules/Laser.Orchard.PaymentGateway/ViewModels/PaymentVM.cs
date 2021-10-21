using Laser.Orchard.PaymentGateway.Models;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.ViewModels {
    public class PaymentVM {
        public List<IPosService> PosList { get; set; }
        public PaymentRecord Record { get; set; }
        public ContentItem ContentItem { get; set; }
        public string PaymentNonce { get; set; }
        public List<AdditionalShapeBase> AdditionalShapes { get; set; }
        public AdditionalShapeContext ShapeContext { get; set; }

        public PaymentVM() {
            PosList = new List<IPosService>();
            Record = new PaymentRecord();
            AdditionalShapes = new List<AdditionalShapeBase>();
        }
    }
}