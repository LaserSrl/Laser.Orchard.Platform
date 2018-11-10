using Orchard;
using Orchard.Localization;
using System;
using System.Globalization;

namespace Laser.Orchard.StartupConfig.Localization {
    public class DateLocalization : IDateLocalization {

        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }


        public DateLocalization(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }



        public DateTime? ReadDateLocalized(DateTime? thedate, bool _useTimeZone = false, string cultureName = "") {
            var currentCulture = cultureName;
            if (currentCulture == "") {
                currentCulture = _orchardServices.WorkContext.CurrentCulture;
            }
            var cultureInfo = CultureInfo.GetCultureInfo(currentCulture);
            var currentTimeZone = _orchardServices.WorkContext.CurrentTimeZone;
            DateTime? date;
            if (_useTimeZone == false) {
                date = thedate == null ? (DateTime?)null : (DateTime)thedate;
            } else {
                date = thedate == null ? (DateTime?)null : TimeZoneInfo.ConvertTimeFromUtc((DateTime)thedate, currentTimeZone);
            }
            return date;
        }

        public string WriteDateLocalized(DateTime? thedate, bool withtime = false, string cultureName = "")
        {
            String formatdate = "d";

            if (withtime)
                formatdate = "g";

            return WriteDateTimeLocalized(thedate, formatdate, cultureName);
        }

        public string WriteTimeLocalized(DateTime? thedate, string cultureName = "")
        {
            return WriteDateTimeLocalized(thedate, "t", cultureName);
        }

        private string WriteDateTimeLocalized(DateTime? thedate, string dateFormat, string cultureName)
        {
            var currentCulture = cultureName;
            if (currentCulture == "")
            {
                currentCulture = _orchardServices.WorkContext.CurrentCulture;
            }
            var cultureInfo = CultureInfo.GetCultureInfo(currentCulture);
            string datestring = thedate.HasValue ? ((DateTime)thedate).ToString(dateFormat, cultureInfo) : String.Empty;
            return datestring;
        }

        public DateTime? StringToDatetime(string date, string time, bool withTimezone = false, string cultureName = "") {
            var currentCulture = cultureName;
            if (String.IsNullOrWhiteSpace(currentCulture)) {
                currentCulture = _orchardServices.WorkContext.CurrentCulture;
            }
            var cultureInfo = CultureInfo.GetCultureInfo(currentCulture);
            var currentTimeZone = _orchardServices.WorkContext.CurrentTimeZone;


            if (!String.IsNullOrWhiteSpace(date)) {
                DateTime theDateoutput;
                var parseTheDateTime = String.Concat(date, " ", time);
                if (DateTime.TryParse(parseTheDateTime, cultureInfo, DateTimeStyles.None, out theDateoutput)) {
                    if (withTimezone == false)
                        return (theDateoutput);
                    else
                        return (TimeZoneInfo.ConvertTimeToUtc(theDateoutput, currentTimeZone));
                } else
                    throw new OrchardException(T("Date not Valid"));

            } else {
                return (null);
            }
        }
    }
}