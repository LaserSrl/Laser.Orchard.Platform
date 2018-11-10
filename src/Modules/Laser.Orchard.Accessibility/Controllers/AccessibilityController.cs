using Laser.Orchard.Accessibility.Services;
using System.Web.Mvc;
using Orchard;

namespace Laser.Orchard.Accessibility.Controllers
{
    public class AccessibilityController : Controller
    {
        private IOrchardServices _orchardServices;
        private IAccessibilityServices _accessibilityServices;

        public AccessibilityController(IOrchardServices services, IAccessibilityServices acServices)
        {
            _orchardServices = services;
            _accessibilityServices = acServices;
        }

        // GET: /Accessibility/
        public ActionResult Index()
        {
            string operation = System.Web.HttpContext.Current.Request.QueryString.ToString();

            switch (operation)
            {
                case "txt":
                    _accessibilityServices.SetTextOnly();
                    break;
                case "high":
                    _accessibilityServices.SetHighContrast();
                    break;
                case "normal":
                    _accessibilityServices.SetNormal();
                    break;
            }

            // Se non arrivo dalla navigazione sulla pagina
            if (_orchardServices.WorkContext.HttpContext.Request.UrlReferrer == null)
                //throw new System.Web.HttpException(404, "Not found");
                return HttpNotFound();
            else {
                // calcola l'url di ritorno: è la pagina in cui è stato richiamato il controller
                // accoda all'url un valore in query string per by-passare la cache del browser
                string acc = operation;
                var reg = new System.Text.RegularExpressions.Regex("(\\?|\\&)acc=[a-z]+(\\&|\\z)");
                string returnUrl = _orchardServices.WorkContext.HttpContext.Request.UrlReferrer.AbsoluteUri;
                var match = reg.Match(returnUrl);
                if (match.Success) {
                    returnUrl = reg.Replace(returnUrl, match.Value.Substring(0, 1) + "acc=" + acc + (match.Value.EndsWith("&")? "&" : ""));
                }
                else if (returnUrl.Contains("?")) {
                    returnUrl += "&acc=" + acc;
                }
                else {
                    returnUrl += "?acc=" + acc;
                }
                return Redirect(returnUrl);
            }
        }
	}
}