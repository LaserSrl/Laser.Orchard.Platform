using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Providers;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Services {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public class CustomPosService : PosServiceBase {
        private readonly IList<ICustomPosProvider> _customPosProviders;
        private readonly IWorkContextAccessor _workContextAccessor;

        public CustomPosService(IOrchardServices orchardServices,
            IRepository<PaymentRecord> repository,
            IPaymentEventHandler paymentEventHandler,
            IShapeFactory shapeFactory,
            IList<ICustomPosProvider> customPosProviders,
            IWorkContextAccessor workContextAccessor) : base(orchardServices, repository, paymentEventHandler, shapeFactory) {
            _customPosProviders = customPosProviders;
            _workContextAccessor = workContextAccessor;
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

        public override string GetPosName(PaymentRecord payment) {
            if (payment.PosName.StartsWith("CustomPos_")) {
                return payment.PosName;
            }
            return base.GetPosName(payment);
        }

        public override string GetPosServiceName(string name) {
            if (name.StartsWith("CustomPos_")) {
                return "Custom Pos";
            }
            return base.GetPosServiceName(name);
        }

        public override string GetPosUrl(int paymentId) {
            return string.Empty;
        }

        public override string GetSettingsControllerName() {
            return "CustomPosAdmin";
        }

        public override IEnumerable<dynamic> GetPaymentButtons() {
            var btns = new List<dynamic>();

            var settings = _orchardServices.WorkContext.CurrentSite.As<CustomPosSiteSettingsPart>();
            if (settings != null) {
                var customPos = settings.CustomPos;

                btns.AddRange(customPos.Select(
                    cp => _shapeFactory.Create(JoinValidStrings("_", "PosPayButton", _customPosProviders
                        .FirstOrDefault(cpp => cpp.TechnicalName.Equals(cp.ProviderName, StringComparison.InvariantCultureIgnoreCase))?.GetButtonShapeName()),
                        Arguments.From(new {
                            CustomPos = cp,
                            PosService = this,
                            PosName = "CustomPos_" + cp.Name
                        }
                    ))));
            }

            return btns;
        }

        public override string GetChargeAdminUrl(PaymentRecord payment) {
            var settings = _orchardServices.WorkContext.CurrentSite.As<CustomPosSiteSettingsPart>();
            if (settings != null) {
                var customPos = settings.CustomPos;

                var pos = customPos.FirstOrDefault(cp => cp.Name.Equals(payment.PosName, StringComparison.InvariantCultureIgnoreCase));
                if (pos != null) {
                    return InnerChargeAdminUrl(payment);
                }
            }

            return base.GetChargeAdminUrl(payment);
        }

        protected override string InnerChargeAdminUrl(PaymentRecord payment) {
            return base.InnerChargeAdminUrl(payment);
        }

        public SelectList GetCustomPosProviders(string selectedProvider) {
            var instances = _customPosProviders;
            List<SelectListItem> lSelectList = new List<SelectListItem>();
            foreach (var instance in instances.OrderByDescending(p => p.GetDisplayName())) {
                lSelectList.Insert(0, new SelectListItem() { Value = instance.TechnicalName, Text = instance.GetDisplayName() });
            }
            return new SelectList((IEnumerable<SelectListItem>)lSelectList, "Value", "Text", selectedProvider);
        }

        /// <summary>
        /// This function works like the standard string.Join but, if one of the parameter is a empty string, it's not added to the result.
        /// </summary>
        /// <param name="separator"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected string JoinValidStrings(string separator, params string[] value) {
            string result = string.Empty;

            foreach (string s in value) {
                if (!string.IsNullOrWhiteSpace(s)) {
                    result += separator + s;
                }
            }

            if (result.StartsWith(separator)) {
                result = result.Substring(1);
            }

            return result;
        }
    }
}