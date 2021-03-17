using Laser.Orchard.Braintree.Controllers;
using Laser.Orchard.Braintree.Models;
using Laser.Orchard.PaymentGateway;
using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Services;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Braintree.Services {
    public class BraintreePosService : PosServiceBase, IBraintreePosService {
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public BraintreePosService(
            IOrchardServices orchardServices, 
            IRepository<PaymentRecord> repository, 
            IPaymentEventHandler paymentEventHandler,
            ICacheManager cacheManager,
            ISignals signals)
            : base(orchardServices, repository, paymentEventHandler) {

            _cacheManager = cacheManager;
            _signals = signals;
        }
        public override string GetPosName() {
            return "Braintree and PayPal";
        }
        public override string GetPosActionUrl(string paymentGuid) {
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return urlHelper.Action("Index", "Braintree", new { area = "Laser.Orchard.Braintree" })
                + "?guid=" + paymentGuid;
        }
        public override string GetPosActionUrl(int paymentId) {
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return urlHelper.Action("Index", "Braintree", new { area = "Laser.Orchard.Braintree" })
                + "?pid=" + paymentId.ToString();
        }
        public override Type GetPosActionControllerType() {
            return typeof(BraintreeController);
        }
        public override string GetPosActionName() {
            return "Index";
        }

        public override string GetPosUrl(int paymentId) {
            throw new NotImplementedException(T("An SDK is available for BrainTree.").Text);
        }

        public override string GetSettingsControllerName() {
            return "Admin";
        }

        public override List<string> GetAllValidCurrencies() {
            return new string[] { GetSettings().CurrencyCode}.ToList();
        }

        protected override string InnerChargeAdminUrl(PaymentRecord payment)
        {
            var config = GetSettings();
            string merchant = config?.MerchantId;
            if (!config.ProductionEnvironment){
                return string.Format("https://sandbox.braintreegateway.com/merchants/{0}/transactions/{1}",
                                     merchant,
                                     payment.TransactionId);
            }
            else {
                return string.Format("https://www.braintreegateway.com/merchants/{0}/transactions/{1}",
                                     merchant,
                                     payment.TransactionId);
            }
        }

        public BraintreeSettings GetSettings() {
            return _cacheManager.Get(BraintreeSiteSettingsPart.CacheKey,
                ctx => {
                    ctx.Monitor(_signals.When(BraintreeSiteSettingsPart.CacheKey));
                    var settingsPart = _orchardServices.WorkContext
                        .CurrentSite.As<BraintreeSiteSettingsPart>();
                    return new BraintreeSettings (settingsPart);
                });
        }
    }
}