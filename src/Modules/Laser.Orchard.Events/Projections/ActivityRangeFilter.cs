using Laser.Orchard.Events.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using System;
using System.Diagnostics;
using System.Globalization;

namespace Laser.Orchard.Events.Projections {

    public interface IFilterProvider : IEventHandler {

        void Describe(dynamic describe);
    }

    public class ActivityRangeFilter : IFilterProvider {

        public Localizer T { get; set; }

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Lazy<CultureInfo> _cultureInfo;

        public ActivityRangeFilter(IWorkContextAccessor workContextAccessor) {
            T = NullLocalizer.Instance;
            _workContextAccessor = workContextAccessor;
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentSite.SiteCulture));
        }

        public void Describe(dynamic describe) {
            describe
                    .For("CalendarEvent", T("Activity Dates"), T("Activity Dates"))
                    .Element("ActivityRange", T("Activity Range"), T("Activities contained in a specified range of dates"),
                             (Action<dynamic>)ApplyFilter,
                             (Func<dynamic, LocalizedString>)DisplayFilter,
                             "ActivityRangeForm");
        }

        public void ApplyFilter(dynamic context) {
            bool error = true;
            var query = (IHqlQuery)context.Query;

            try {
                if (!string.IsNullOrEmpty((string)context.State.DateFrom) || !string.IsNullOrEmpty((string)context.State.DateTo)) {
                    var dateFrom = new DateTime(1900, 1, 1);
                    if (!string.IsNullOrEmpty((string)context.State.DateFrom))
                        dateFrom = Convert.ToDateTime((string)context.State.DateFrom, _cultureInfo.Value);
                    else if (!string.IsNullOrEmpty((string)context.State.DateTo))
                        dateFrom = Convert.ToDateTime((string)context.State.DateTo, _cultureInfo.Value);

                    var dateTo = new DateTime(2100, 12, 31);
                    if (!string.IsNullOrEmpty((string)context.State.DateTo)) {
                        dateTo = Convert.ToDateTime((string)context.State.DateTo, _cultureInfo.Value);
                        if (((string)context.State.DateTo).Length == 10)
                            dateTo = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day, 23, 59, 59);
                    }
                    else if (!string.IsNullOrEmpty((string)context.State.DateFrom)) {
                        dateTo = Convert.ToDateTime((string)context.State.DateFrom, _cultureInfo.Value);
                        dateTo = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day, 23, 59, 59);
                    }
                    if (dateFrom > dateTo) {
                        error = true;
                    }
                    else {
                        context.Query = query.Where(
                                                    x => x.ContentPartRecord<ActivityPartRecord>(),
                                                    f => f.Disjunction(
                                                            fDateTimeNoRepeat => fDateTimeNoRepeat.Conjunction(
                                                                x => x.Eq("Repeat", false),
                                                                x => x.Le("DateTimeStart", dateTo),
                                                                x => x.Ge("DateTimeEnd", dateFrom),
                                                                x => x.LeProperty("DateTimeStart", "activityPartRecord.DateTimeEnd")
                                                            ),
                                                            fDateTimeRepeat => fDateTimeRepeat.Conjunction(
                                                                x => x.Eq("Repeat", true),
                                                                x => x.Eq("RepeatEnd", false),
                                                                x => x.Le("DateTimeStart", dateTo)
                                                            ),
                                                            fDateTimeRepeatEnd => fDateTimeRepeatEnd.Conjunction(
                                                                x => x.Eq("Repeat", true),
                                                                x => x.Eq("RepeatEnd", true),
                                                                x => x.Ge("RepeatEndDate", dateFrom),
                                                                x => x.Le("DateTimeStart", dateTo)
                                                            )
                                                        )
                                                    );
                        error = false;
                    }
                }
            }
            catch (Exception e) {
                Debug.Print(e.Message);
            }

            if (error) {
                //Query returning nothing
                context.Query = query.Where(
                                            x => x.ContentPartRecord<ActivityPartRecord>(),
                                            f => f.And(x => x.Eq("AllDay", true), x => x.Eq("AllDay", false)));
            }

            return;
        }

        public LocalizedString DisplayFilter(dynamic context) {
            return T("Activities contained in a specified range of dates");
        }
    }
}