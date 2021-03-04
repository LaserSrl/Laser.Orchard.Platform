using System;
using Orchard.Conditions.Services;
using Orchard.Environment.Configuration;
using Orchard.Mvc;

namespace Laser.Orchard.StartupConfig.Rules {
    /// <summary>
    /// Use in Widget Layer Definition as: httpheader('header-key','header-value')
    /// You can use * as wildcard char at the beginning or at the end of the header value to match partial values
    /// i.e.
    /// if Request Header User-Agent is Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.190 Safari/537.36
    /// httpheader('User-Agent','Mozilla/5.0*') returns true (header begins with)
    /// httpheader('User-Agent','*Safari/537.36') returns true (header ends with)
    /// httpheader('User-Agent','*Windows NT*') returns true (header contains)
    /// </summary>
    public class HttpHeaderCondition : IConditionProvider {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ShellSettings _shellSettings;
        private const string WILDCARD = "*";
        public HttpHeaderCondition(IHttpContextAccessor httpContextAccessor, ShellSettings shellSettings) {
            _httpContextAccessor = httpContextAccessor;
            _shellSettings = shellSettings;
        }

        public void Evaluate(ConditionEvaluationContext evaluationContext) {
            if (!string.Equals(evaluationContext.FunctionName, "httpheader", StringComparison.OrdinalIgnoreCase))
                return;
            if (evaluationContext.Arguments.Length != 2) {
                return;
            }
            var context = _httpContextAccessor.Current();
            var headerKey = evaluationContext.Arguments[0].ToString();
            var headerValue = evaluationContext.Arguments[1].ToString();
            var currentHeaderValue = context.Request.Headers[headerKey];
            if (currentHeaderValue != null) {
                if (currentHeaderValue == headerValue) {
                    evaluationContext.Result = true;
                    return;
                }
                else if (headerValue.StartsWith(WILDCARD) || headerValue.EndsWith(WILDCARD)) {
                    if (headerValue.StartsWith(WILDCARD) && headerValue.EndsWith(WILDCARD) && currentHeaderValue.Contains(headerValue.Trim(WILDCARD.ToCharArray()))) {
                        evaluationContext.Result = true;
                        return;
                    }
                    else if (headerValue.StartsWith(WILDCARD) && currentHeaderValue.EndsWith(headerValue.Trim(WILDCARD.ToCharArray()))) {
                        evaluationContext.Result = true;
                        return;
                    }
                    else if (headerValue.EndsWith(WILDCARD) && currentHeaderValue.StartsWith(headerValue.Trim(WILDCARD.ToCharArray()))) {
                        evaluationContext.Result = true;
                        return;
                    }
                }
            }
            evaluationContext.Result = false;
        }
    }
}