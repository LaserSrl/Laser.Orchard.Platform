using Laser.Orchard.Braintree.Models;
using Laser.Orchard.Braintree.Services;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using Bt = Braintree;

namespace Laser.Orchard.Braintree.Controllers
{
    [WebApiKeyFilter(true)]
    public class Paypal : ApiController
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IBraintreeService _braintreeService;

        public Paypal(IOrchardServices orchardServices, IBraintreeService braintreeService)
        {
            _orchardServices = orchardServices;
            _braintreeService = braintreeService;
        }

        public HttpResponseMessage Get()
        {
            var clientToken = _braintreeService.GetClientToken();

            // create response and return 
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new System.Net.Http.StringContent(clientToken, Encoding.UTF8, "text/plain");
            return result;
        }

        public HttpResponseMessage Post()
        {
            string esito = "";
            var locRequest = HttpContext.Current.Request;
            string nonce = locRequest["payment_method_nonce"];
            string sAmount = locRequest["amount"];
            decimal amount = decimal.Parse(sAmount, CultureInfo.InvariantCulture);

            var payResult = _braintreeService.Pay(nonce, amount, null);
            esito = (payResult.Success) ? "OK" : "KO";

            // create response and return 
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new System.Net.Http.StringContent(esito, Encoding.UTF8, "text/plain");
            return result;
        }
    }
}