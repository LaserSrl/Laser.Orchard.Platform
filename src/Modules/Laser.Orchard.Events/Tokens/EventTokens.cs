using Laser.Orchard.Events.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Laser.Orchard.Events.Tokens
{
    public interface ITokenProvider : IEventHandler
    {
        void Describe(DescribeContext context);
        void Evaluate(EvaluateContext context);
    }

    public class EventTokens : ITokenProvider
    {
        private readonly ICurrentContentAccessor _currentContentAccessor;
        private readonly ITokenizer _tokenizer;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Lazy<CultureInfo> _cultureInfo;

        public Localizer T { get; set; }

        public EventTokens(ICurrentContentAccessor currentContentAccessor, ITokenizer tokenizer, IWorkContextAccessor workContextAccessor)
        {
            _currentContentAccessor = currentContentAccessor;
            _tokenizer = tokenizer;
            _workContextAccessor = workContextAccessor;
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentSite.SiteCulture));
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context)
        {
            context.For("Content", T("Event List"), T("Tokens for Calendars (list shape)"))
                   .Token("CalendarStartDate", T("List Start Date"), T("Start date of the parent Calendar"))
                   .Token("CalendarEndDate", T("List End Date"), T("End date of the parent Calendar"));
        }

        public void Evaluate(EvaluateContext context)
        {
            if (_currentContentAccessor.CurrentContentItem == null)
                return;
            else if (_currentContentAccessor.CurrentContentItem.As<CalendarPart>() == null)
                return;

            context.For("Content", _currentContentAccessor.CurrentContentItem)
                   .Token("CalendarStartDate", content => GetStartDate(content))
                   .Chain("CalendarStartDate", "Date", content => { DateTime dateResult; if (DateTime.TryParse(GetStartDate(content), _cultureInfo.Value, DateTimeStyles.None, out dateResult)) return dateResult; else return null; })
                   .Token("CalendarEndDate", content => GetEndDate(content))
                   .Chain("CalendarEndDate", "Date", content => { DateTime dateResult; if (DateTime.TryParse(GetEndDate(content), _cultureInfo.Value, DateTimeStyles.None, out dateResult)) return dateResult; else return null; });
        }

        private string GetStartDate(ContentItem content)
        {
            CalendarPart calendar = content == null ? null : content.As<CalendarPart>();

            if (calendar == null)
                return null;
            else
                return _tokenizer.Replace(calendar.StartDate, new Dictionary<string, object>());
        }

        private string GetEndDate(ContentItem content)
        {
            CalendarPart calendar = content == null ? null : content.As<CalendarPart>();

            if (calendar == null)
                return null;
            else {
                DateTime startDate = Convert.ToDateTime(_tokenizer.Replace(calendar.StartDate, new Dictionary<string, object>()), _cultureInfo.Value);

                int duration = 1; //Valore standard da usare se la conversione fallisce
                int.TryParse(_tokenizer.Replace(calendar.NumDays, new Dictionary<string, object>()), out duration);
                if (duration <= 0) duration = 1;

                return startDate.AddDays(duration - 1).ToString(_cultureInfo.Value);
            }
        }
    }
}