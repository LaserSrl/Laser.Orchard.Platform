using Newtonsoft.Json;
using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Couponing;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Controllers {
    [OutputCache(NoStore = true, Duration = 0)]
    public class CouponRESTApiController : ApiController {

        private readonly ICouponApplicationService _couponApplicationService;
        private readonly IShoppingCart _shoppingCart;
        private readonly IWorkContextAccessor _workContextAccessor;

        public CouponRESTApiController(
            ICouponApplicationService couponApplicationService,
            IShoppingCart shoppingCart,
            IWorkContextAccessor workContextAccessor) {

            _couponApplicationService = couponApplicationService;
            _shoppingCart = shoppingCart;
            _workContextAccessor = workContextAccessor;
        }

        public IHttpActionResult Post(CouponFrontendViewModel coupon) {
            // try to add the coupon to the cart
            if (coupon != null && !string.IsNullOrWhiteSpace(coupon.Code)) {
                var context = new CouponApplicabilityContext {
                    CouponCode = coupon.Code,
                    ShoppingCart = _shoppingCart,
                    WorkContext = _workContextAccessor.GetContext(),
                };
                _couponApplicationService.ApplyCoupon(context);
                return SuccessResponse(context);
            }
            return BadRequest();
        }

        public IHttpActionResult Delete(CouponFrontendViewModel coupon) {
            // try to remove the coupon from the cart
            if (coupon != null && !string.IsNullOrWhiteSpace(coupon.Code)) {
                var context = new CouponApplicabilityContext {
                    CouponCode = coupon.Code,
                    ShoppingCart = _shoppingCart,
                    WorkContext = _workContextAccessor.GetContext(),
                };
                _couponApplicationService.RemoveCoupon(context);
                return SuccessResponse(context);
            }
            return BadRequest();
        }

        public IHttpActionResult Options() {
            var msg = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("")
            };
            msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            msg.Content.Headers.Allow.Add("POST");
            msg.Content.Headers.Allow.Add("DELETE");
            return ResponseMessage(msg);
        }

        private IHttpActionResult SuccessResponse(CouponApplicabilityContext context) {
            var msg = new HttpResponseMessage(HttpStatusCode.OK);
            var result = new OperationResult {
                Success = context.IsApplicable,
                Message = context.Message != null
                    ? context.Message.Text
                    : string.Empty
            };
            msg.Content = new StringContent(
                JsonConvert
                    .SerializeObject(result)
                    .ToString());
            msg.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/json");
            return ResponseMessage(msg);
        }

        class OperationResult {
            public bool Success { get; set; }
            public string Message { get; set; }
        }
    }
}