using Laser.Orchard.PaymentGateway;
using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Services;
using Laser.Orchard.PayPal.Controllers;
using Orchard;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PayPal.Services {
    public class PayPalPosService : PosServiceBase {
        public PayPalPosService(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler)
           : base(orchardServices, repository, paymentEventHandler) {
        }

        public override string GetPosName() {
            return "PayPal";
        }

        public override string GetPosActionUrl(string paymentGuid) {
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return urlHelper.Action("Index", "PayPal", new { area = "Laser.Orchard.PayPal" })
                + "?guid=" + paymentGuid;
        }

        public override string GetPosActionUrl(int paymentId) {
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return urlHelper.Action("Index", "PayPal", new { area = "Laser.Orchard.PayPal" })
                + "?pid=" + paymentId.ToString();
        }

        public override Type GetPosActionControllerType() {
            return typeof(PayPalController);
        }

        public override string GetPosActionName() {
            return "Index";
        }

        public override string GetPosUrl(int paymentId) {
            throw new NotImplementedException();
        }

        public override string GetSettingsControllerName() {
            return "Admin";
        }

        protected override string InnerChargeAdminUrl(PaymentRecord payment) {
            // temporarily put the activity section because PayPal does not return the transaction id
            return "https://www.sandbox.paypal.com/myaccount/transactions";
        }
    }
}