using Laser.Orchard.Accessibility.Services;
using Orchard;
using Orchard.Mvc.Filters;
using Orchard.Security;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Orchard.UI.Admin;

namespace Laser.Orchard.Accessibility.Filters
{
    public class TextOnlyFilter : FilterProvider, IActionFilter, IResultFilter
    {
        private readonly IOrchardServices _orchardServices;
        private TextWriter _originalWriter;
        private Action<ControllerContext> _completeResponse;
        private StringWriter _tempWriter;

        public TextOnlyFilter(IOrchardServices services, IAccessibilityServices accServices)
        {
            _orchardServices = services;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool isAdmin = AdminFilter.IsApplied(filterContext.RequestContext);
            if (!isAdmin) {
                _originalWriter = filterContext.HttpContext.Response.Output;
                _tempWriter = new StringWriterWithEncoding(_originalWriter.Encoding, _originalWriter.FormatProvider);
                filterContext.HttpContext.Response.Output = _tempWriter;
                _completeResponse = CaptureResponse;
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (_completeResponse != null)
            {
                _completeResponse(filterContext);
            }
        }

        private void CaptureResponse(ControllerContext filterContext)
        {
            filterContext.HttpContext.Response.Output = _originalWriter;

            string capturedText = _tempWriter.ToString();
            _tempWriter.Dispose();

            // se richiesto, pulisce l'output per avere "solo testo"
            // controlla sia il cookie sia il fatto di non avere accesso alla dashboard
            bool isAdmin = AdminFilter.IsApplied(filterContext.RequestContext);
            if (new Utils().GetTenantCookieValue(Utils.AccessibilityCookieName, filterContext.HttpContext.Request) == Utils.AccessibilityTextOnly
                && (isAdmin == false))
            {
                // pulizia dell'html per ottenere l'effetto "solo testo"
                capturedText = clearResult(capturedText);
            }
            filterContext.HttpContext.Response.Write(capturedText);
        }

        private string clearResult(string result)
        {
            Regex regex = null;
            string[] tags = { "img", "script", "link", "style" };
            string[] tagReplace = { "", "noscript", "", "noscript" };

            for (int idx=0; idx<tags.Length; idx++)
            {
                if (string.IsNullOrEmpty(tagReplace[idx]))
                {
                    // apertura tag
                    regex = new Regex("<(" + tags[idx] + ")([^>]*)>", RegexOptions.IgnoreCase);
                    result = regex.Replace(result, "");

                    // chiusura tag
                    regex = new Regex("</(" + tags[idx] + ")([^>]*)>", RegexOptions.IgnoreCase);
                    result = regex.Replace(result, "");
                }
                else
                {
                    // apertura tag
                    regex = new Regex("<(" + tags[idx] + ")([^>]*)>", RegexOptions.IgnoreCase);
                    result = regex.Replace(result, "<" + tagReplace[idx] + ">");

                    // chiusura tag
                    regex = new Regex("</(" + tags[idx] + ")([^>]*)>", RegexOptions.IgnoreCase);
                    result = regex.Replace(result, "</" + tagReplace[idx] + ">");
                }
            }

            return result;
        }
    }
}