using System;
using System.Linq;
using System.Web;
using Orchard.Localization;

namespace Orchard.Tokens.Providers {
    public class ReplaceToken : ITokenProvider {
        public ReplaceToken() {
            T = NullLocalizer.Instance;

        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Text")
                .Token("Replace:*", T("Replace:<(oldcharacters|newcharacters)>"), T("Replace the specified old characters with the new characters."))
                ;
        }

        public void Evaluate(EvaluateContext context) {
            context.For<String>("Text", () => "")
                // {Text.Replace:<"oldchars"|"newchar">}
                .Token(FilterTokenParam,
                    (fullToken, data) => { return Replace(fullToken, data, context); })
                .Chain(FilterChainParam, "Text", (token, data) => { return Replace(token, data, context); });
        }

        private string Replace(string fullToken, string data, EvaluateContext context) {
            string[] chars = fullToken.Split('|');
            if (chars.Length < 2) {
                return "";
            }
            return data.Replace(chars[0], chars[1]);
        }

        private static string FilterTokenParam(string token) {
            string tokenPrefix;
            int chainIndex, tokenLength;
            if (token.IndexOf(":") == -1) {
                return null;
            }
            tokenPrefix = token.Substring(0, token.IndexOf(":"));
            if (!tokenPrefix.Equals("Replace", StringComparison.InvariantCultureIgnoreCase)) {
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
            if (!tokenPrefix.Equals("Replace", StringComparison.InvariantCultureIgnoreCase)) {
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
    }
}