using Laser.Orchard.Events.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Laser.Orchard.Events.Tokens
{
    public class ActivityPartTokens : ITokenProvider
    {
        private readonly ICurrentContentAccessor _currentContentAccessor;
        private readonly ITokenizer _tokenizer;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Lazy<CultureInfo> _cultureInfo;
        private readonly IDateLocalizationServices _dateLocalizationServices;

        public Localizer T { get; set; }

        public ActivityPartTokens(
            ICurrentContentAccessor currentContentAccessor, 
            ITokenizer tokenizer, 
            IWorkContextAccessor workContextAccessor,
            IDateLocalizationServices dateLocalizationServices)
        {
            _currentContentAccessor = currentContentAccessor;
            _tokenizer = tokenizer;
            _workContextAccessor = workContextAccessor;
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentSite.SiteCulture));
            T = NullLocalizer.Instance;
            _dateLocalizationServices = dateLocalizationServices;
        }

        public void Describe(DescribeContext context)
        {
            context.For("Content")
                   .Token("StartDate", T("List Start Date"), T("Start date of Activity"))
                   .Token("EndDate", T("List End Date"), T("End date of Activity"));
        }

        public void Evaluate(EvaluateContext context)
        {
            //if (_currentContentAccessor.CurrentContentItem == null)
            //    return;
            //else if (_currentContentAccessor.CurrentContentItem.As<ActivityPart>() == null)
            //    return;
            context.For<IContent>("Content") 
                   .Token(
                        token => GetDateFormat(token), 
                        (token, content) => GetStartDate(content, token))
                    .Chain(FilterChainParam, "Date", (token, data) => { return FindProperty(token, data, context); })
                   .Token("EndDate", content => "endDate");
        }

        //StartDate:dd-MM-yyyyThh:mm:ss
        //    Se AllDay > non formattare il time
        //    sennò formatta

        //StartDate:yyyy-MM-ddThh:mm

        //Chain > DateTime
        //    StartDate:(dd-MM-yyyyThh:mm:ss).Add:Days,2 => 2019
        private string GetDateFormat(string token) {
            if (token.StartsWith("ActivityStartDate:", StringComparison.OrdinalIgnoreCase)) {
                return token.Substring("ActivityStartDate:".Length);
            } else if(token.StartsWith("ActivityStartDate", StringComparison.OrdinalIgnoreCase)) {
                return token.Substring("ActivityStartDate".Length);
            } else {
                return null;
            }
        }

        private string GetStartDate(IContent content, string token)
        {
            ActivityPart activity = content == null ? null : content.As<ActivityPart>();
            if (activity == null)
                return null;
            else {
                return ParseDate(activity, token, activity.DateTimeStart.Value);
            }
        }

        private string ParseDate(ActivityPart activity, string token, DateTime date) {
            bool allDay = activity.AllDay;
            string dateFormat = string.Empty;
            if (String.IsNullOrEmpty(token)) {
                if(allDay == true) {
                    dateFormat = "yyyy-MM-dd";
                } else {
                    dateFormat = "yyyy-MM-ddTHH:mm";
                }
                return _dateLocalizationServices.ConvertToLocalizedString(date, dateFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false });
            } else {
                dateFormat = token;
            }

            return _dateLocalizationServices.ConvertToLocalizedString(date, dateFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false });
        }

        private object FindProperty(string fullToken, IContent data, EvaluateContext context) {
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
            } catch {
                return "parameter error";
            }
            return "parameter not found";
        }

        private static Tuple<string, string> FilterChainParam(string token) {
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