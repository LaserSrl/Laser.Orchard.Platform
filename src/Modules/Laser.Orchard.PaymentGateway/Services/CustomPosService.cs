using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.PaymentGateway.Services {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public class CustomPosService : PosServiceBase {
        public CustomPosService(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler) : base(orchardServices, repository, paymentEventHandler) {
        }

        public override Type GetPosActionControllerType() {
            return typeof(object);
        }

        public override string GetPosActionName() {
            return "Index";
        }

        public override string GetPosActionUrl(int paymentId) {
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return urlHelper.Action("Index", "CustomPos", new { area = "Laser.Orchard.PaymentGateway" })
                + "?pid=" + paymentId.ToString();
        }

        public override string GetPosActionUrl(string paymentGuid) {
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return urlHelper.Action("Index", "CustomPos", new { area = "Laser.Orchard.PaymentGateway" })
                + "?guid=" + paymentGuid;
        }

        public override string GetPosName() {
            return "Custom Pos";
        }

        public override string GetPosUrl(int paymentId) {
            return string.Empty;
        }

        public override string GetSettingsControllerName() {
            return "CustomPosAdmin";
        }
    }
}