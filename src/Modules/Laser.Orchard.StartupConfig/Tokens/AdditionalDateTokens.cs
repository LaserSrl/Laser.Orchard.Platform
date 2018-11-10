using System;
using System.Linq;
using Orchard.Taxonomies.Fields;
using Orchard.Localization;
using Orchard.Tokens;
using System.Globalization;
using Orchard;
using Orchard.Services;

namespace Laser.Orchard.StartupConfig.Tokens {
    public class AdditionalDateTokens : ITokenProvider {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IClock _clock;

        //private DateTime _resultDate;
        private string _fullTokenName;
        public AdditionalDateTokens(IWorkContextAccessor workContextAccessor, IClock clock) {
            _workContextAccessor = workContextAccessor;
            _clock = clock;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {

            context.For("Date", T("Date/time"), T("Current date/time tokens"))
                .Token("Add:*(n)", T("Add:Days|Months|Years to a date"), T("Add Days|Months|Years to a date"), "Date");
        }

        public void Evaluate(EvaluateContext context) {
            context.For<DateTime>("Date", () => _clock.UtcNow)
                .Token(
                token => token.StartsWith("Add:", StringComparison.OrdinalIgnoreCase) ? token.Substring(0, (token.IndexOf(".") > 0 ? token.IndexOf(".") : token.Length)) : "",
                    (token, d) => {
                        _fullTokenName = token;
                        return GetDatebyToken(token, d);
                    }
                )
                .Chain(_fullTokenName, "Date", (d) => GetDatebyToken(_fullTokenName, d)); // il nome del token passato alla chain deve essere identico al token presente nel primo parametro del Token(tokeName,...,...)
        }

        private DateTime? GetDatebyToken(string token, DateTime originalDate) {
            if (token == "") return null;
            var parenthesysPosition = token.IndexOf("(");
            var function = token.Substring(0, parenthesysPosition);
            var number = token.Substring(parenthesysPosition + 1, token.Length - parenthesysPosition - 2);
            if (function.Equals("Add:Days", StringComparison.InvariantCultureIgnoreCase)) {
                return originalDate.AddDays(Convert.ToDouble(number));
            } else if (function.Equals("Add:Months", StringComparison.InvariantCultureIgnoreCase)) {
                return originalDate.AddMonths(Convert.ToInt32(number));
            } else if (function.Equals("Add:Years", StringComparison.InvariantCultureIgnoreCase)) {
                return originalDate.AddYears(Convert.ToInt32(number));
            } else {
                return null;
            }
        
        
        }
    }
}