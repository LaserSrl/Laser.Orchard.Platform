﻿using Laser.Orchard.PaymentGateway;
using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Services;
using Laser.Orchard.PayPal.Controllers;
using Laser.Orchard.PayPal.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using System;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PayPal.Services {
    public class PayPalPosService : PosServiceBase {
        public PayPalPosService(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler, IShapeFactory shapeFactory)
           : base(orchardServices, repository, paymentEventHandler, shapeFactory) {
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
            var config = _orchardServices.WorkContext.CurrentSite.As<PayPalSiteSettingsPart>();

            if (config.ProductionEnvironment) {
                return "https://www.paypal.com/myaccount/transactions";
            } else {
                return "https://www.sandbox.paypal.com/myaccount/transactions";
            }
        }
    }
}