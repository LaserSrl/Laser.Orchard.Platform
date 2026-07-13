using Orchard;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Laser.Orchard.StartupConfig.Tokens {
    public class UnescapedQueryStringToken : ITokenProvider {
        private readonly IWorkContextAccessor _workContextAccessor;
        private static string[] _textChainableTokens;

        public UnescapedQueryStringToken(IWorkContextAccessor workContextAccessor) { 
        
            _workContextAccessor = workContextAccessor;
            _textChainableTokens = new string[] { "QueryString", "Form", "Header", "UnescapedQueryString" };
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Request", T("Http Request"), T("Current Http Request tokens."))
                .Token("UnescapedQueryString:*", 
                    T("UnescapedQueryString:<element>"), 
                    T("The Query String value for the specified element, unescape. To chain text, surround the <element> with parentheses, e.g. 'QueryString:(param1)'."));
        }

        public void Evaluate(EvaluateContext context) {
            if (_workContextAccessor.GetContext().HttpContext == null) {
                return;
            }

            context.For("Request", _workContextAccessor.GetContext().HttpContext.Request)
                .Token(
                    token => token.StartsWith("UnescapedQueryString:", StringComparison.OrdinalIgnoreCase) ? FilterTokenParam(token) : null,
                    (token, request) => {
                        return Uri.UnescapeDataString(request.QueryString.Get(token) ?? "");
                    }
                )
                .Chain(token => token.StartsWith("UnescapedQueryString:", StringComparison.OrdinalIgnoreCase) ? FilterChainParam(token) : null,
                    "Text", (token, request) => Uri.UnescapeDataString(request.QueryString.Get(token) ?? ""));
        }

        private static string FilterTokenParam(string token) {
            string tokenPrefix;
            int chainIndex, tokenLength;
            if (token.IndexOf(":") == -1) {
                return null;
            }
            tokenPrefix = token.Substring(0, token.IndexOf(":"));
            if (!_textChainableTokens.Contains(tokenPrefix, StringComparer.OrdinalIgnoreCase)) {
                return null;
            }

            // use ")." as chars combination to discover the end of the parameter
            chainIndex = token.IndexOf(").") + 1;
            tokenLength = (tokenPrefix + ":").Length;
            if (chainIndex == 0) {// ")." has not be found
                return token.Substring(tokenLength).Trim(new char[] { '(', ')' });
            }
            if (!token.StartsWith((tokenPrefix + ":"), StringComparison.OrdinalIgnoreCase) || chainIndex <= tokenLength) {
                return null;
            }
            return token.Substring(tokenLength, chainIndex - tokenLength).Trim(new char[] { '(', ')' });
        }

        private static Tuple<string, string> FilterChainParam(string token) {
            string tokenPrefix;
            int chainIndex, tokenLength;

            if (token.IndexOf(":") == -1) {
                return null;
            }
            tokenPrefix = token.Substring(0, token.IndexOf(":"));
            if (!_textChainableTokens.Contains(tokenPrefix, StringComparer.OrdinalIgnoreCase)) {
                return null;
            }

            // use ")." as chars combination to discover the end of the parameter
            chainIndex = token.IndexOf(").") + 1;
            tokenLength = (tokenPrefix + ":").Length;
            if (chainIndex == 0) { // ")." has not be found
                return new Tuple<string, string>(token.Substring(tokenLength).Trim(new char[] { '(', ')' }), "");
            }
            if (!token.StartsWith((tokenPrefix + ":"), StringComparison.OrdinalIgnoreCase) || chainIndex <= tokenLength) {
                return null;
            }
            return new Tuple<string, string>(token.Substring(tokenLength, chainIndex - tokenLength).Trim(new char[] { '(', ')' }), token.Substring(chainIndex + 1));

        }

        private static string RemoveDiacritics(string text) {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++) {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark) {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }
    }
}