using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Laser.Orchard.Events.Models;
using Laser.Orchard.Events.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Tokens;
using Orchard.UI.Navigation;

namespace Laser.Orchard.Events.Services {
    public class EventsService : IEventsService {
        private readonly IOrchardServices _orchardServices;
        private readonly IProjectionManager _projectionManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ITokenizer _tokenizer;
        private readonly Lazy<CultureInfo> _cultureInfo;

        public EventsService(IProjectionManager projectionManager, ITokenizer tokenizer, IWorkContextAccessor workContextAccessor, IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            _projectionManager = projectionManager;
            _tokenizer = tokenizer;
            _workContextAccessor = workContextAccessor;
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentSite.SiteCulture));
        }

        public List<EventViewModel> GetEvents(int queryId, DateTime intervalStart, DateTime intervalEnd, CalendarPart part = null, bool addDayForThreshold = false) {
            List<EventViewModel> list = new List<EventViewModel>();

            //Recupero tutti gli eventi dalla query associata al calendario
            //var eventArray = _projectionManager.GetContentItems(queryId, 0, 0, part == null ? null : new Dictionary<string, object> { { "Content", part } }).ToList();
            var eventArray = _projectionManager.GetContentItems(queryId, 0, 0); //TODO: issue when we want to do tokenized queries from widgets

            //Recupero gli eventi senza ripetizione e li aggiungo al calendario
            var listNoRepeat = new List<EventViewModel>();
            listNoRepeat = eventArray
                .Where(w => ((dynamic)w).ActivityPart.Repeat == false)
                .Select(s =>
                 new EventViewModel {
                     content = ((dynamic)s),
                     start = ((dynamic)s).ActivityPart.AllDay ? ((dynamic)s).ActivityPart.DateTimeStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : ((dynamic)s).ActivityPart.DateTimeStart.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture),
                     end = ((dynamic)s).ActivityPart.AllDay ?
                                (addDayForThreshold ?
                                    ((dynamic)s).ActivityPart.DateTimeEnd.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                                    : ((dynamic)s).ActivityPart.DateTimeEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                                )
                                : ((dynamic)s).ActivityPart.DateTimeEnd.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture)
                 }
            )
            .ToList();
            list.AddRange(listNoRepeat);

            //Recupero gli eventi con ripetizione giornaliera ed elaboro la loro aggiunta al calendario
            var listDayRepeat = new List<ContentItem>();
            listDayRepeat = eventArray
                .Where(w => ((dynamic)w).ActivityPart.Repeat == true && ((dynamic)w).ActivityPart.RepeatType == "D")
                .ToList();

            foreach (var item in listDayRepeat) {
                bool allday = ((dynamic)item).ActivityPart.AllDay;

                DateTime eventStartDate = ((dynamic)item).ActivityPart.DateTimeStart;
                DateTime eventEndDate = ((dynamic)item).ActivityPart.DateTimeEnd;

                int eventDuration = (eventEndDate.Date - eventStartDate.Date).Days;

                //Uniformo la rappresentazione delle date, poiché in fullcalendar se allDay vale true il giorno finale è incluso e se vale false invece è escluso
                if (allday && addDayForThreshold)
                    eventDuration += 1;

                DateTime startDate = new DateTime(Math.Max(intervalStart.AddDays(-eventDuration).Ticks, eventStartDate.Ticks));

                DateTime endDate;
                if (((dynamic)item).ActivityPart.RepeatEnd)
                    endDate = new DateTime(Math.Min(intervalEnd.Ticks, (((dynamic)item).ActivityPart.RepeatEndDate).Ticks));
                else
                    endDate = intervalEnd;

                string startTimePart = "";
                string endTimePart = "";

                if (!allday) {
                    startTimePart = "T" + eventStartDate.ToString("HH:mm", CultureInfo.InvariantCulture);

                    if (!eventEndDate.ToString("HH:mm", CultureInfo.InvariantCulture).Equals("00:00"))
                        endTimePart = "T" + eventEndDate.ToString("HH:mm", CultureInfo.InvariantCulture);
                    else
                        endTimePart = "T23:59";
                }

                DateTime loopDate = startDate;
                while (DateTime.Compare(loopDate.Date, endDate.Date) <= 0) {
                    list.Add(new EventViewModel {
                        content = ((dynamic)item),
                        start = loopDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + startTimePart,
                        end = loopDate.AddDays(eventDuration).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + endTimePart
                    });

                    loopDate = loopDate.AddDays(((dynamic)item).ActivityPart.RepeatValue);
                }
            }

            //Recupero gli eventi con ripetizione settimanale ed elaboro la loro aggiunta al calendario
            var listWeekRepeat = new List<ContentItem>();
            listWeekRepeat = eventArray
                .Where(w => ((dynamic)w).ActivityPart.Repeat == true && ((dynamic)w).ActivityPart.RepeatType == "W")
                .ToList();

            foreach (var item in listWeekRepeat) {
                bool allday = ((dynamic)item).ActivityPart.AllDay;

                DateTime eventStartDate = ((dynamic)item).ActivityPart.DateTimeStart;
                DateTime eventEndDate = ((dynamic)item).ActivityPart.DateTimeEnd;

                int eventDuration = (eventEndDate.Date - eventStartDate.Date).Days;

                //Uniformo la rappresentazione delle date, poiché in fullcalendar se allDay vale true il giorno finale è incluso e se vale false invece è escluso
                if (allday && addDayForThreshold)
                    eventDuration += 1;

                string startTimePart = "";
                string endTimePart = "";

                if (!allday) {
                    startTimePart = "T" + eventStartDate.ToString("HH:mm", CultureInfo.InvariantCulture);

                    if (!eventEndDate.ToString("HH:mm", CultureInfo.InvariantCulture).Equals("00:00"))
                        endTimePart = "T" + eventEndDate.ToString("HH:mm", CultureInfo.InvariantCulture);
                    else
                        endTimePart = "T23:59";
                }

                string repeatDetails = ((dynamic)item).ActivityPart.RepeatDetails;

                DateTime loopDate = intervalStart.AddDays(-eventDuration);
                while (DateTime.Compare(loopDate.Date, intervalEnd.Date) <= 0) {
                    bool repeat = ((dynamic)item).ActivityPart.RepeatEnd ? DateTime.Compare(loopDate.Date, ((dynamic)item).ActivityPart.RepeatEndDate.Date) <= 0 : true;

                    int weeksPassed = (loopDate.Date - eventStartDate.Date).Days / 7;

                    if (repeatDetails.Contains(loopDate.DayOfWeek.ToString())
                        && DateTime.Compare(loopDate.Date, eventStartDate.Date) >= 0
                        && (weeksPassed % ((dynamic)item).ActivityPart.RepeatValue) == 0
                        && repeat) {
                        list.Add(new EventViewModel {
                            content = ((dynamic)item),
                            start = loopDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + startTimePart,
                            end = loopDate.AddDays(eventDuration).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + endTimePart
                        });
                    }

                    loopDate = loopDate.AddDays(1);
                }
            }

            //Recupero gli eventi con ripetizione mensile ed elaboro la loro aggiunta al calendario
            var listMonthRepeat = new List<ContentItem>();
            listMonthRepeat = eventArray
                .Where(w => ((dynamic)w).ActivityPart.Repeat == true && ((dynamic)w).ActivityPart.RepeatType == "M")
                .ToList();

            foreach (var item in listMonthRepeat) {
                bool allday = ((dynamic)item).ActivityPart.AllDay;

                DateTime eventStartDate = ((dynamic)item).ActivityPart.DateTimeStart;
                DateTime eventEndDate = ((dynamic)item).ActivityPart.DateTimeEnd;

                int eventDuration = (eventEndDate.Date - eventStartDate.Date).Days;

                //Uniformo la rappresentazione delle date, poiché in fullcalendar se allDay vale true il giorno finale è incluso e se vale false invece è escluso
                if (allday && addDayForThreshold)
                    eventDuration += 1;

                string startTimePart = "";
                string endTimePart = "";

                if (!allday) {
                    startTimePart = "T" + eventStartDate.ToString("HH:mm", CultureInfo.InvariantCulture);

                    if (!eventEndDate.ToString("HH:mm", CultureInfo.InvariantCulture).Equals("00:00"))
                        endTimePart = "T" + eventEndDate.ToString("HH:mm", CultureInfo.InvariantCulture);
                    else
                        endTimePart = "T23:59";
                }

                DateTime loopEnd;
                if (((dynamic)item).ActivityPart.RepeatEnd)
                    loopEnd = new DateTime(Math.Min(intervalEnd.Ticks, (((dynamic)item).ActivityPart.RepeatEndDate).Ticks));
                else
                    loopEnd = intervalEnd;

                DateTime loopDate = eventStartDate;

                //Ripetizione per numero di mese
                if (((dynamic)item).ActivityPart.RepeatDetails.Contains("DayNum")) {
                    int monthDifference = 0;
                    while (DateTime.Compare(loopDate.Date, loopEnd.Date) <= 0) {
                        if (DateTime.Compare(loopDate.Date, intervalStart.Date) >= 0
                            || (DateTime.Compare(loopDate.Date, intervalStart.Date) < 0 && DateTime.Compare(loopDate.AddDays(eventDuration).Date, intervalStart.Date) >= 0)) {
                            list.Add(new EventViewModel {
                                content = ((dynamic)item),
                                start = loopDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + startTimePart,
                                end = loopDate.AddDays(eventDuration).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + endTimePart
                            });
                        }

                        //Calcolo la differenza in mesi a partire dalla data iniziale ed effettuo l'aggiunta di mesi a partire da essa
                        //Se non facessi così, ad esempio un loop sul 30 del mese verrebbe spostato al 28/29 dopo Febbraio
                        monthDifference = (loopDate.Year - eventStartDate.Year) * 12 + loopDate.Month - eventStartDate.Month;
                        loopDate = eventStartDate.AddMonths(monthDifference + ((dynamic)item).ActivityPart.RepeatValue);
                    }
                }
                else if (((dynamic)item).ActivityPart.RepeatDetails.Contains("DayWeek")) {
                    int nthInMonth = (int)Math.Ceiling(Decimal.Divide(eventStartDate.Day, 7));

                    while (DateTime.Compare(loopDate.Date, loopEnd.Date) <= 0) {
                        DateTime eventDate;
                        if (DateTime.Compare(loopDate.Date, eventStartDate.Date) == 0)
                            eventDate = eventStartDate;
                        else
                            eventDate = getNthDayOfMonth(eventStartDate.DayOfWeek, nthInMonth, loopDate.Month, loopDate.Year);

                        if (DateTime.Compare(eventDate.Date, intervalStart.Date) >= 0
                            || (DateTime.Compare(eventDate.Date, intervalStart.Date) < 0 && DateTime.Compare(eventDate.AddDays(eventDuration).Date, intervalStart.Date) >= 0)) {
                            list.Add(new EventViewModel {
                                content = ((dynamic)item),
                                start = eventDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + startTimePart,
                                end = eventDate.AddDays(eventDuration).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + endTimePart
                            });
                        }

                        loopDate = loopDate.AddMonths(((dynamic)item).ActivityPart.RepeatValue);
                    }
                }
            }

            //Recupero gli eventi con ripetizione annuale ed elaboro la loro aggiunta al calendario
            var listYearRepeat = new List<ContentItem>();
            listYearRepeat = eventArray
                .Where(w => ((dynamic)w).ActivityPart.Repeat == true && ((dynamic)w).ActivityPart.RepeatType == "Y")
                .ToList();

            foreach (var item in listYearRepeat) {
                bool allday = ((dynamic)item).ActivityPart.AllDay;

                DateTime eventStartDate = ((dynamic)item).ActivityPart.DateTimeStart;
                DateTime eventEndDate = ((dynamic)item).ActivityPart.DateTimeEnd;

                int eventDuration = (eventEndDate.Date - eventStartDate.Date).Days;

                //Uniformo la rappresentazione delle date, poiché in fullcalendar se allDay vale true il giorno finale è incluso e se vale false invece è escluso
                if (allday && addDayForThreshold)
                    eventDuration += 1;

                string endTimePart = "";
                if (!allday) {
                    if (!eventEndDate.ToString("HH:mm", CultureInfo.InvariantCulture).Equals("00:00"))
                        endTimePart = "T" + eventEndDate.ToString("HH:mm", CultureInfo.InvariantCulture);
                    else
                        endTimePart = "T23:59";
                }

                DateTime loopDate;
                int loopEnd = ((dynamic)item).ActivityPart.RepeatEnd ? Math.Min(intervalEnd.Year, (((dynamic)item).ActivityPart.RepeatEndDate).Year) : intervalEnd.Year;

                for (int year = eventStartDate.Year; year <= loopEnd; year = year + ((dynamic)item).ActivityPart.RepeatValue) {
                    loopDate = new DateTime(year, eventStartDate.Month, eventStartDate.Day, eventStartDate.Hour, eventStartDate.Minute, eventStartDate.Second);

                    bool repeat = ((dynamic)item).ActivityPart.RepeatEnd ? DateTime.Compare(loopDate.Date, ((dynamic)item).ActivityPart.RepeatEndDate.Date) <= 0 : true;

                    if (DateTime.Compare(loopDate.Date, intervalEnd.Date) <= 0
                        && (DateTime.Compare(loopDate.Date, intervalStart.Date) >= 0 || (DateTime.Compare(loopDate.Date, intervalStart.Date) < 0 && DateTime.Compare(loopDate.AddDays(eventDuration).Date, intervalStart.Date) >= 0))
                        && repeat) {
                        list.Add(new EventViewModel {
                            content = ((dynamic)item),
                            start = allday ? loopDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : loopDate.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture),
                            end = loopDate.AddDays(eventDuration).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + endTimePart
                        });
                    }
                }
            }

            return list;
        }

        public List<EventViewModel> GetEventList(CalendarPart part) {
            try {
                string start = _tokenizer.Replace(part.StartDate, new Dictionary<string, object>());

                if (!String.IsNullOrEmpty(start)) {
                    int duration = 1; //Valore standard da usare se la conversione fallisce
                    int.TryParse(_tokenizer.Replace(part.NumDays, new Dictionary<string, object>()), out duration);
                    if (duration <= 0) duration = 1;

                    DateTime startDate = Convert.ToDateTime(start, _cultureInfo.Value);
                    return GetEvents(part.QueryPartRecord.Id, startDate, startDate.AddDays(duration - 1), part);
                }
                else
                    return null;
            }
            catch {
                return null;
            }
        }

        public List<EventViewModel> OrderEventList(List<EventViewModel> list, CalendarPart part) {
            List<SortCriterionRecord> sortCriteriaList = part.QueryPartRecord.SortCriteria.ToList();

            if (sortCriteriaList != null) {
                sortCriteriaList = sortCriteriaList.Where(x => x.Category == "ActivityPart").OrderBy(x => x.Position).ToList();

                var orderedList = list.OrderBy(x => 0);
                foreach (SortCriterionRecord sortRecord in sortCriteriaList) {
                    switch (sortRecord.Type) {
                        case "DateTimeStart": {
                                if (sortRecord.State.Contains("<Sort>true</Sort>"))
                                    orderedList = orderedList.ThenBy(x => x.start);
                                else if (sortRecord.State.Contains("<Sort>false</Sort>"))
                                    orderedList = orderedList.ThenByDescending(x => x.start);

                                break;
                            }
                        case "DateTimeEnd": {
                                if (sortRecord.State.Contains("<Sort>true</Sort>"))
                                    orderedList = orderedList.ThenBy(x => x.end);
                                else if (sortRecord.State.Contains("<Sort>false</Sort>"))
                                    orderedList = orderedList.ThenByDescending(x => x.end);

                                break;
                            }
                    }
                }

                return orderedList.ToList();
            }
            else
                return list;
        }

        public List<AggregatedEventViewModel> GetAggregatedList(CalendarPart part) {
            List<AggregatedEventViewModel> aggregatedList = new List<AggregatedEventViewModel>();
            List<EventViewModel> eventList = GetEventList(part);
            eventList = OrderEventList(eventList, part);

            if (eventList != null) {
                foreach (var item in eventList) {
                    if (aggregatedList.Any(x => ((dynamic)x).Content.Id == item.content.Id)) {
                        var itemToUpdate = aggregatedList.Single(x => ((dynamic)x).Content.Id == item.content.Id);
                        itemToUpdate.Occurrences.Add(new Occurrence { Start = item.start, End = item.end });
                    }
                    else {
                        List<Occurrence> occurrencesList = new List<Occurrence>();
                        occurrencesList.Add(new Occurrence { Start = item.start, End = item.end });

                        aggregatedList.Add(new AggregatedEventViewModel {
                            Content = item.content,
                            Occurrences = occurrencesList
                        });
                    }
                }
            }

            return aggregatedList;
        }

        public List<AggregatedEventViewModel> GetAggregatedList(CalendarPart part, int page, int pageSize) {
            List<AggregatedEventViewModel> aggregatedList = GetAggregatedList(part);

            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, page, pageSize);
            aggregatedList = aggregatedList.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();

            return aggregatedList;
        }

        private DateTime getNthDayOfMonth(DayOfWeek dayOfWeek, int occurrence, int month, int year) {
            DateTime date = new DateTime(year, month, 1);

            while (date.DayOfWeek != dayOfWeek)
                date = date.AddDays(1);

            if (occurrence <= 4)
                date = date.AddDays(7 * (occurrence - 1));
            else {
                //Prendo l'ultimo del mese
                date = date.AddDays(7 * 3);
                if (date.AddDays(7).Month == month)
                    date = date.AddDays(7);
            }

            return date;
        }
    }
}