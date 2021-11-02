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

        public CustomPosService(IOrchardServices orchardServices,
            IRepository<PaymentRecord> repository,
            IPaymentEventHandler paymentEventHandler,
            IShapeFactory shapeFactory,
            IList<ICustomPosProvider> customPosProviders) : base(orchardServices, repository, paymentEventHandler, shapeFactory) {
            _customPosProviders = customPosProviders;
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

        public override IEnumerable<dynamic> GetPaymentButtons() {
            var btns = new List<dynamic>();

            var settings = _orchardServices.WorkContext.CurrentSite.As<CustomPosSiteSettingsPart>();
            if (settings != null) {
                var customPos = settings.CustomPos;

                btns.AddRange(customPos.Select(
                    cp => _shapeFactory.Create("PosPayButton_" + _customPosProviders
                        .FirstOrDefault(cpp => cpp.TechnicalName.Equals(cp.ProviderName, StringComparison.InvariantCultureIgnoreCase)).GetButtonShapeName(),
                        Arguments.From(new {
                            CustomPos = cp,
                            PosService = this,
                            PosName = cp.Name
                        }
                    ))));
            }

            return btns;
        }

        public SelectList GetCustomPosProviders(string selectedProvider) {
            var instances = _customPosProviders;
            List<SelectListItem> lSelectList = new List<SelectListItem>();
            foreach (var instance in instances.OrderByDescending(p => p.GetDisplayName())) {
                lSelectList.Insert(0, new SelectListItem() { Value = instance.TechnicalName, Text = instance.GetDisplayName() });
            }
            return new SelectList((IEnumerable<SelectListItem>)lSelectList, "Value", "Text", selectedProvider);
        }
    }
}