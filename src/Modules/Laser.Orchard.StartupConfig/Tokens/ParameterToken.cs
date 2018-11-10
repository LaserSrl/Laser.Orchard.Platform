using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Tokens {
    public class ParameterToken : ITokenProvider {

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Content")
                   .Token("Parameter:*", T("Parameter:<PartName-PropertyName>"), T("Return the property value of a part specified"));
        }

        public void Evaluate(EvaluateContext context) {
            //If you want to Chain TextTokens you have to use the (PartName-PropertyName).SomeOtherTextToken
            context.For<IContent>("Content")
                .Token(FilterTokenParam, //t.StartsWith("Parameter:", StringComparison.OrdinalIgnoreCase) ? t.Substring("Parameter:".Length) : null,
                    (fullToken, data) => { return FindProperty(fullToken, data, context); })
                .Chain(FilterChainParam, "Text", (token, data) => { return FindProperty(token, data, context); });
        }

        private string FindProperty(string fullToken, IContent data, EvaluateContext context) {
            string[] names = fullToken.Split('-');
            ContentItem content = data.ContentItem;

            if(names.Length < 2) {
                return "";
            }

            string partName = names[0];
            string propName = names[1];
            try {
                foreach (var part in content.Parts) {
                    string partType = part.GetType().ToString().Split('.').Last();
                    if (partType.Equals(partName, StringComparison.InvariantCultureIgnoreCase)) {
                        return part.GetType().GetProperty(propName).GetValue(part, null).ToString();
                    }
                }
            }catch {
                return "parameter error";
            }
            return "parameter not found";
        }

        private static string FilterTokenParam(string token) {
            string tokenPrefix;
            int chainIndex, tokenLength;
            if (token.IndexOf(":") == -1) {
                return null;
            }
            tokenPrefix = token.Substring(0, token.IndexOf(":"));
            if (!tokenPrefix.Equals("Parameter", StringComparison.InvariantCultureIgnoreCase)) {
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
            if (!tokenPrefix.Equals("Parameter", StringComparison.InvariantCultureIgnoreCase)){
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