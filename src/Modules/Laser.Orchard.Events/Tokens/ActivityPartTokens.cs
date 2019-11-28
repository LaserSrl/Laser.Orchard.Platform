using Laser.Orchard.Events.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Tokens;
using System;
using System.Globalization;

namespace Laser.Orchard.Events.Tokens {
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
                   .Token("ActivityStartDate", T("Activity Start Date"), T("Start date of Activity"))
                   .Token("ActivityEndDate", T("Activity End Date"), T("End date of Activity"))
                   .Token("ActivityStartDateString", T("Activity Start Date (as String)"), T("String representing the start date of Activity"))
                   .Token("ActivityEndDateString", T("Activity End Date (as String)"), T("String representing the end date of Activity"));
        }

        public void Evaluate(EvaluateContext context)
        {
            context.For<IContent>("Content")
                .Token(token =>
                    token.Equals("ActivityStartDateString", StringComparison.InvariantCultureIgnoreCase) ? String.Empty : null,
                    (token, content) => {
                        return GetStartDateString(content, token);
                    }
                )
                .Chain(
                    StartDateStringChainParam,
                    "Text",
                    (token, content) => {
                        return GetStartDateString(content, token);
                    }
                )
                .Token(token => TokenStartDateStringParam(token),
                    (token, content) => {
                        return GetStartDateString(content, token);
                    }
                )
                .Chain(
                    StartDateStringChainParam,
                    "Text",
                    (token, content) => {
                        return GetStartDateString(content, token);
                    }
                )
                .Token(token =>
                    token.Equals("ActivityEndDateString", StringComparison.InvariantCultureIgnoreCase) ? String.Empty : null,
                    (token, content) => {
                        return GetEndDateString(content, token);
                    }
                )
                .Chain(
                    EndDateStringChainParam,
                    "Text",
                    (token, content) => {
                        return GetEndDateString(content, token);
                    }
                )
                .Token(token => TokenEndDateStringParam(token),
                    (token, content) => {
                        return GetEndDateString(content, token);
                    }
                )
                .Chain(
                    EndDateStringChainParam,
                    "Text",
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
                )
                .Chain(
                    EndDateChainParam,
                    "Date",
                    (token, data) => {
                        return GetEndDate(data);
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

            string format = token.Split(':', ')')[1];
            if (String.IsNullOrEmpty(format))
                return null;

            return format.Trim(new char[] { '(', ')' });
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
            if (chainIndex == 0) {// ")." has not be found
                //if chain has not value get part of token with date format
                tokenPrefix = token.Substring(0, token.IndexOf(":"));
                tokenLength = (tokenPrefix + ":").Length;
                return token.Substring(tokenLength).Trim(new char[] { '(', ')' });
            }

            tokenPrefix = token.Substring(0, token.IndexOf(")"));
            tokenLength = tokenPrefix.Length; 
            if (chainIndex <= tokenLength) {
                return null;
            }

            string format = token.Split(':', ')')[1];
            if (String.IsNullOrEmpty(format))
                return null;

            return format.Trim(new char[] { '(', ')' });
        }

        private static Tuple<string, string> StartDateChainParam(string token) {
            if (!token.StartsWith("ActivityStartDate)", StringComparison.OrdinalIgnoreCase)) {
                if (!token.Equals("ActivityStartDate", StringComparison.InvariantCultureIgnoreCase)) {
                    return null;
                }
            }

            return EvaluateChainDateFormat(token);
        }
        private static Tuple<string, string> EndDateChainParam(string token) {
            if (!token.StartsWith("ActivityEndDate)", StringComparison.OrdinalIgnoreCase)) {
                if (!token.Equals("ActivityEndDate", StringComparison.InvariantCultureIgnoreCase)) {
                    return null;
                }
            }

            return EvaluateChainDateFormat(token);
        }

        private static Tuple<string, string> StartDateStringChainParam(string token) {
            if (token.StartsWith("ActivityStartDateString", StringComparison.InvariantCultureIgnoreCase)) {
                if (token.IndexOf("ActivityStartDateString:") != -1) {
                    //check if format is written after ':' char
                    int formatIndex = token.IndexOf(":") + 1;
                    if (formatIndex > 0) {
                        if (String.IsNullOrEmpty(token.Substring(formatIndex, token.IndexOf(")"))))
                            return null;
                    }
                }

                return EvaluateChainStringFormat(token);
            }

            return null;
        }
        private static Tuple<string, string> EndDateStringChainParam(string token) {
            string format;

            if (token.StartsWith("ActivityEndDateString", StringComparison.InvariantCultureIgnoreCase)) {
                if (token.IndexOf("ActivityEndDateString:") != -1) {
                    //check if format is written after ':' char
                    format = token.Split(':', ')')[1];
                    if (String.IsNullOrEmpty(format))
                        return null;
                }

                return EvaluateChainStringFormat(token);
            }

            return null;
        }

        private static Tuple<string, string> EvaluateChainDateFormat(string token) {
            string tokenPrefix;
            int chainIndex, tokenLength, tokenLimit;

            tokenLimit = token.IndexOf(")");
            // use ")." as chars combination to discover the end of the parameter
            chainIndex = token.IndexOf(").") + 1;
            if (chainIndex == 0) { // ")." has not be found
                tokenLength = tokenLimit == 0 ? token.Length : tokenLimit;

                return new Tuple<string, string>(token.Substring(tokenLength).Trim(new char[] { '(', ')' }), "");
            }

            tokenPrefix = token.Substring(0, tokenLimit);
            tokenLength = tokenPrefix.Length;

            if (chainIndex <= tokenLength) {
                return null;
            }

            //ActivityStartDate).Format:yyyy-MM-dd
            return new Tuple<string, string>(
                // ActivityStartDate
                String.Empty,
                // Format:yyyy-MM-dd
                token.Substring(chainIndex + 1));
        }

        private static Tuple<string, string> EvaluateChainStringFormat(string token) {
            string tokenPrefix, format;
            int chainIndex, tokenLength;

            chainIndex = token.IndexOf(").") + 1;
            if (chainIndex == 0) { // ")." has not be found
                return new Tuple<string, string>(token.Substring(token.Length).Trim(new char[] { '(', ')' }), "");
            }

            tokenPrefix = token.Substring(0, token.IndexOf(")"));
            tokenLength = tokenPrefix.Length;

            if (chainIndex <= tokenLength) {
                return null;
            }

            if (tokenPrefix.IndexOf(":") == -1) {
                return new Tuple<string, string>(
                    "",
                    token.Substring(chainIndex + 1));
            } else {
                format = token.Split(':', ')')[1];
                return new Tuple<string, string>(
                    format.Trim(new char[] { '(', ')' }),
                    token.Substring(chainIndex + 1));
            }
        }
    }
}