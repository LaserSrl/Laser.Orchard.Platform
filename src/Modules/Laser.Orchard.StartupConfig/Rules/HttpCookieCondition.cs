using System;
using Orchard.Conditions.Services;
using Orchard.Environment.Configuration;
using Orchard.Mvc;

namespace Laser.Orchard.StartupConfig.Rules {
    /// <summary>
    /// Use in Widget Layer Definition as: httpcookie('cookie-key','cookie-value')
    /// You can use * as wildcard char at the beginning or at the end of the cookie value to match partial values
    /// i.e.
    /// if Request Cookie cultureData is currentCulture=it-IT
    /// httpcookie('cultureData','current*') returns true (cookie begins with)
    /// httpcookie('cultureData','*-IT') returns true (cookie ends with)
    /// httpcookie('cultureData','*Culture=it*') returns true (cookie contains)
    /// </summary>
    public class HttpCookieCondition : IConditionProvider {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ShellSettings _shellSettings;
        private const string WILDCARD = "*";
        public HttpCookieCondition(IHttpContextAccessor httpContextAccessor, ShellSettings shellSettings) {
            _httpContextAccessor = httpContextAccessor;
            _shellSettings = shellSettings;
        }

        public void Evaluate(ConditionEvaluationContext evaluationContext) {
            if (!string.Equals(evaluationContext.FunctionName, "httpcookie", StringComparison.OrdinalIgnoreCase))
                return;
            if (evaluationContext.Arguments.Length != 2) {
                return;
            }
            var context = _httpContextAccessor.Current();
            var cookieName = evaluationContext.Arguments[0].ToString();
            var cookieValue = evaluationContext.Arguments[1].ToString();
            var currentCookieValue = context.Request.Cookies[cookieName]?.Value;
            if (currentCookieValue != null) {
                if (currentCookieValue == cookieValue) {
                    evaluationContext.Result = true;
                    return;
                }
                else if (cookieValue.StartsWith(WILDCARD) || cookieValue.EndsWith(WILDCARD)) {
                    if (cookieValue.StartsWith(WILDCARD) && cookieValue.EndsWith(WILDCARD) && currentCookieValue.Contains(cookieValue.Trim(WILDCARD.ToCharArray()))) {
                        evaluationContext.Result = true;
                        return;
                    }
                    else if (cookieValue.StartsWith(WILDCARD) && currentCookieValue.EndsWith(cookieValue.Trim(WILDCARD.ToCharArray()))) {
                        evaluationContext.Result = true;
                        return;
                    }
                    else if (cookieValue.EndsWith(WILDCARD) && currentCookieValue.StartsWith(cookieValue.Trim(WILDCARD.ToCharArray()))) {
                        evaluationContext.Result = true;
                        return;
                    }
                }
            }
            evaluationContext.Result = false;
        }
    }
}