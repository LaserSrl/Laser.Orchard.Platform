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
        private string _fullTokenName;
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
                .Token(token =>
                    token.Equals("ActivityStartDateString", StringComparison.InvariantCultureIgnoreCase) ? String.Empty : null,
                    (token, content) => {
                        return GetStartDateString(content, token);
                    }
                )
                .Token(token => TokenStartDateStringParam(token),
                    (token, content) => {
                        return GetStartDateString(content, token);
                    }
                )
                .Token(token =>
                    token.Equals("ActivityEndDateString", StringComparison.InvariantCultureIgnoreCase) ? String.Empty : null,
                    (token, content) => {
                        return GetStartDateString(content, token);
                    }
                )
                .Token(token => TokenEndDateStringParam(token),
                    (token, content) => {
                        return GetEndDateString(content, token);
                    }
                )
                .Token(token =>
                    token.Equals("ActivityStartDate", StringComparison.InvariantCultureIgnoreCase) ? String.Empty : null,
                    (token, content) => {
                        return GetStartDate(content);
                    }
                )
                .Chain(
                    StartDateChainParam, 
                    "Date", 
                    (token, data) => {
                        return GetStartDate(data);
                    }
                )
                .Token(token =>
                    token.Equals("ActivityEndDate", StringComparison.InvariantCultureIgnoreCase) ? String.Empty : null,
                    (token, content) => {
                        return GetEndDate(content);
                    }
                );
        }

        //get datetime from start date of activity
        private object GetStartDate(IContent content) {
            ActivityPart activity = content == null ? null : content.As<ActivityPart>();
            if (activity == null)
                return null;
            else if(activity.DateTimeStart.Value != DateTime.MinValue) { 
                return activity.DateTimeStart.Value;
            }

            return null; 
        }
        
        //get datetime from end date of activity
        private object GetEndDate(IContent content) {
            ActivityPart activity = content == null ? null : content.As<ActivityPart>();
            if (activity == null)
                return null;
            else if (activity.DateTimeEnd.Value != DateTime.MinValue) {
                return activity.DateTimeEnd.Value;
            }

            return null;
        }

        //methods to get string of formatted date           
        private object GetStartDateString(IContent content, string dateFormat) {
            ActivityPart activity = content == null ? null : content.As<ActivityPart>();
            if (activity == null)
                return null;
            else {
                return ParseDate(activity, activity.DateTimeStart.Value, dateFormat);
            }
        }
        private object GetEndDateString(IContent content, string dateFormat) {
            ActivityPart activity = content == null ? null : content.As<ActivityPart>();
            if (activity == null)
                return null;
            else {
                return ParseDate(activity, activity.DateTimeEnd.Value, dateFormat);
            }
        }

        //method to parse date with passed format or not
        private string ParseDate(ActivityPart activity, DateTime date, string format) {
            bool allDay = activity.AllDay;
            string dateFormat = string.Empty;
            if (String.IsNullOrEmpty(format)) {
                if (allDay == true) {
                    dateFormat = "yyyy-MM-dd";
                } else {
                    dateFormat = "yyyy-MM-ddTHH:mm";
                }
            } else {
                dateFormat = format;
            }
            return _dateLocalizationServices.ConvertToLocalizedString(date, dateFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false });
        }

        private static string TokenStartDateStringParam(string token) {
            string tokenPrefix;
            int chainIndex, tokenLength;

            //check if token equal to token start date string
            if (!token.StartsWith(("ActivityStartDateString:"), StringComparison.OrdinalIgnoreCase)) {
                return null;
            }

            // use ")." as chars combination to discover the end of the parameter
            chainIndex = token.IndexOf(").") + 1;
            tokenLength = token.Length;
            if (chainIndex == 0) {// ")." has not be found
                //if chain has not value get part of token with date format
                tokenPrefix = token.Substring(0, token.IndexOf(":"));
                tokenLength = (tokenPrefix + ":").Length;
                return token.Substring(tokenLength).Trim(new char[] { '(', ')' });
            }

            if (chainIndex <= tokenLength) {
                return null;
            }
            return token.Substring(tokenLength, chainIndex - tokenLength).Trim(new char[] { '(', ')' });
        }

        private static string TokenEndDateStringParam(string token) {
            string tokenPrefix;
            int chainIndex, tokenLength;

            //check if token equal to token start date string
            if (!token.StartsWith(("ActivityEndDateString:"), StringComparison.OrdinalIgnoreCase)) {
                return null;
            }

            // use ")." as chars combination to discover the end of the parameter
            chainIndex = token.IndexOf(").") + 1;
            tokenLength = token.Length;
            if (chainIndex == 0) {// ")." has not be found
                //if chain has not value get part of token with date format
                tokenPrefix = token.Substring(0, token.IndexOf(":"));
                tokenLength = (tokenPrefix + ":").Length;
                return token.Substring(tokenLength).Trim(new char[] { '(', ')' });
            }

            if (chainIndex <= tokenLength) {
                return null;
            }
            return token.Substring(tokenLength, chainIndex - tokenLength).Trim(new char[] { '(', ')' });
        }

        private static Tuple<string, string> StartDateChainParam(string token) {
            string tokenPrefix;
            int chainIndex, tokenLength;

            tokenPrefix = token.Substring(0, token.IndexOf(")."));
            if (!tokenPrefix.Equals("ActivityStartDate", StringComparison.InvariantCultureIgnoreCase)) {
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