using Laser.Orchard.StartupConfig.Localization;
using Orchard;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SEO.Services {
    public interface ISEOServices : IDependency {
        DateTime DateToLocal(DateTime? utcDate);
        DateTime DateToUTC(DateTime? localDate);
        DateTime DateToUTC(string localDate);
        DateTime LocalDateFromString(string date);
        string DateToString(DateTime date);
    }

    public class SEOServices : ISEOServices {

        private readonly IDateLocalizationServices _dateServices;
        private readonly IDateLocalization _dateLocalization;
        private readonly IOrchardServices _orchardServices;

        public SEOServices(IDateLocalizationServices dateServices, IDateLocalization dateLocalization, IOrchardServices orchardServices) {
            _dateServices = dateServices;
            _dateLocalization = dateLocalization;
            _orchardServices = orchardServices;
        }
        /// <summary>
        /// Get the local time from its UTC representation.
        /// </summary>
        /// <param name="utcDate">UTC-based time.</param>
        /// <returns>Local time.</returns>
        public DateTime DateToLocal(DateTime? utcDate) {
            if (utcDate == null || utcDate.Value <= (DateTime)SqlDateTime.MinValue) {
                utcDate = SqlDateTime.MinValue.Value.AddDays(1);
            }
            else if (utcDate >= (DateTime)SqlDateTime.MaxValue) {
                utcDate = SqlDateTime.MaxValue.Value.Subtract(TimeSpan.FromDays(1));
            }
            return (DateTime)_dateServices.ConvertToSiteTimeZone(utcDate.Value);
        }
        /// <summary>
        /// Get the UTC time from its local representation
        /// </summary>
        /// <param name="localDate">Local time</param>
        /// <returns>UTC-based time</returns>
        public DateTime DateToUTC(DateTime? localDate) {
            if (localDate == null || localDate.Value <= (DateTime)SqlDateTime.MinValue) {
                localDate = SqlDateTime.MinValue.Value.AddDays(1);
            }
            else if (localDate >= (DateTime)SqlDateTime.MaxValue) {
                localDate = SqlDateTime.MaxValue.Value.Subtract(TimeSpan.FromDays(1));
            }
            return (DateTime)(_dateServices.ConvertFromLocalizedString(_dateLocalization.WriteDateLocalized(localDate), _dateLocalization.WriteTimeLocalized(localDate)));

        }
        public DateTime DateToUTC(string localDate) {
            return _dateServices.ConvertFromLocalizedDateString(localDate).Value;
        }

        public DateTime LocalDateFromString(string date) {
            DateTime? dt = _dateServices.ConvertFromLocalizedDateString(date);
            if (dt == null) {
                return DateTime.MinValue;
            }
            return _dateServices.ConvertToSiteTimeZone(dt.Value);
        }

        public string DateToString(DateTime date) {
            var culture = new System.Globalization.CultureInfo(_orchardServices.WorkContext.CurrentCulture);

            return date.ToString("d", culture);
        }
    }
}