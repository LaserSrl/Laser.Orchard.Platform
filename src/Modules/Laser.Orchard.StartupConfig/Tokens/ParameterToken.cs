using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.ServiceModel.Channels;

namespace Laser.Orchard.StartupConfig.Tokens {
    public class ParameterToken : ITokenProvider {

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Content")
                   .Token("Parameter:*", T("Parameter:<PartName-PropertyName>"), T("Return the property value of a part specified"))
                   .Token("DateTimeParameter:*", T("DateTimeParameter:<PartName-PropertyName>"), T("Return the property value of a part specified"))
                   .Token("UrlParameter:*", T("UrlParameter:<PartName-PropertyName>"), T("Return the property value of a part specified"))
                   .Token("CultureParameter:*", T("CultureParameter:<PartName-PropertyName>"), T("Return the property value of a part specified"));
        }

        public void Evaluate(EvaluateContext context) {
            //If you want to Chain TextTokens you have to use the (PartName-PropertyName).SomeOtherTextToken

            context.For<IContent>("Content")
                .Token(FilterTokenParam, //t.StartsWith("Parameter:", StringComparison.OrdinalIgnoreCase) ? t.Substring("Parameter:".Length) : null,
                    (fullToken, data) => { return FindProperty(fullToken, data, context); })
                .Chain(FilterChainParam, "Text", (token, data) => { return FindProperty(token, data, context); })
                .Token(FilterTokenDateTimeParam, //t.StartsWith("DateTimeParameter:", StringComparison.OrdinalIgnoreCase) ? t.Substring("Parameter:".Length) : null,
                    (fullToken, data) => { return FindProperty(fullToken, data, context); })
                .Chain(FilterChainDateTimeParam, "Date", (token, data) => {
                    DateTime? property = FindDateTimeProperty(token, data, context);
                    return property.HasValue ? property : null; })
                .Token(FilterTokenUrlParam, //t.StartsWith("UrlParameter:", StringComparison.OrdinalIgnoreCase) ? t.Substring("Parameter:".Length) : null,
                    (fullToken, data) => { return FindProperty(fullToken, data, context); })
                .Chain(FilterChainUrlParam, "Url", (token, data) => {return FindUrlProperty(token, data, context);})
                .Token(FilterTokenCultureParam, //t.StartsWith("CultureParameter:", StringComparison.OrdinalIgnoreCase) ? t.Substring("Parameter:".Length) : null,
                    (fullToken, data) => { return FindProperty(fullToken, data, context); })
                .Chain(FilterChainCultureParam, "Culture", (token, data) => { return FindCultureProperty(token, data, context); });
        }

        private string FindProperty(string fullToken, IContent data, EvaluateContext context) {

            dynamic property = FindTypedProperty(fullToken, data, context);
            return property.ToString();            
        }

        private DateTime? FindDateTimeProperty(string fullToken, IContent data, EvaluateContext context) {

            dynamic property = FindTypedProperty(fullToken, data, context);

            if (property.GetType() != typeof(DateTime)) {
                return null;
            }
            return property;
        }

        private Uri FindUrlProperty(string fullToken, IContent data, EvaluateContext context) {

            dynamic property = FindTypedProperty(fullToken, data, context);

            if (property.GetType() != typeof(Uri)) {
                return null;
            }
            return property;
        }

        private string FindCultureProperty(string fullToken, IContent data, EvaluateContext context) {           
            return FindProperty(fullToken, data, context);
        }

        private dynamic FindTypedProperty(string fullToken, IContent data, EvaluateContext context) {
            string[] names = fullToken.Split('-');
            ContentItem content = data.ContentItem;

            if (names.Length < 2) {
                return "";
            }

            string partName = names[0];
            string propName = names[1];
            try {
                foreach (var part in content.Parts) {
                    string partType = part.GetType().ToString().Split('.').Last();
                    if (partType.Equals(partName, StringComparison.InvariantCultureIgnoreCase)) {
                        return part.GetType().GetProperty(propName).GetValue(part, null);
                    }
                }
            }
            catch {
                return null;
            }
            return null;
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


        private string FilterTokenDateTimeParam(string token) {
            return FilterTokenTypedParam(token, "DateTime");
        }

        private string FilterTokenUrlParam(string token) {
            return FilterTokenTypedParam(token, "Url");
        }

        private string FilterTokenCultureParam(string token) {
            return FilterTokenTypedParam(token, "Culture");
        }

        private string FilterTokenTypedParam(string token, string typePrefix) {
            string tokenPrefix;
            int chainIndex, tokenLength;

            if (token.IndexOf(":") == -1) {
                return null;
            }
            tokenPrefix = token.Substring(0, token.IndexOf(":"));
            if (!tokenPrefix.Equals(typePrefix + "Parameter", StringComparison.InvariantCultureIgnoreCase)) {
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

        private Tuple<string, string> FilterChainDateTimeParam(string token) {
            return FilterChainTypedParam(token, "DateTime");
        }

        private Tuple<string, string> FilterChainUrlParam(string token) {
            return FilterChainTypedParam(token, "Url");
        }

        private Tuple<string, string> FilterChainCultureParam(string token) {
            return FilterChainTypedParam(token, "Culture");
        }

        private Tuple<string, string> FilterChainTypedParam(string token, string typePrefix) {
            string tokenPrefix;
            int chainIndex, tokenLength;
          
            if (token.IndexOf(":") == -1) {
                return null;
            }
            tokenPrefix = token.Substring(0, token.IndexOf(":"));
            if (!tokenPrefix.Equals(typePrefix + "Parameter", StringComparison.InvariantCultureIgnoreCase)) {
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