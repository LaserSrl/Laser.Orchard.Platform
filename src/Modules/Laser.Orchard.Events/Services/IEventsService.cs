using Laser.Orchard.Events.Models;
using Laser.Orchard.Events.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.Events.Services
{
    public interface IEventsService : IDependency
    {
        /// <summary>
        /// Gets a list of events between two dates
        /// </summary>
        /// <param name="queryId">The id of the query to launch.</param>
        /// <param name="intervalStart">The starting date of the interval.</param>
        /// <param name="intervalEnd">The ending date of the interval.</param>
        /// <param name="part">Optional: the entire content part to pass to the tokenizer.</param>
        /// <param name="addDayForThreshold">Optional: true if dates without timestamp must be increased by one day to go beyond fullcalendar threshold.</param>
        List<EventViewModel> GetEvents(int queryId, DateTime intervalStart, DateTime intervalEnd, CalendarPart part = null, bool addDayForThreshold = false);

        /// <summary>
        /// Gets a list of events using the interval infos contained into the part (List shape only)
        /// </summary>
        /// <param name="part">The calendar part.</param>
        List<EventViewModel> GetEventList(CalendarPart part);

        /// <summary>
        /// Orders the list of events using the sorting infos contained into the part (List shape only)
        /// </summary>
        /// <param name="part">The calendar part.</param>
        List<EventViewModel> OrderEventList(List<EventViewModel> list, CalendarPart part);

        /// <summary>
        /// Gets a list of events in aggregated form using the interval infos contained into the part (List shape only)
        /// </summary>
        /// <param name="part">The calendar part.</param>
        List<AggregatedEventViewModel> GetAggregatedList(CalendarPart part);

        /// <summary>
        /// Gets a paginated list of events in aggregated form using the interval infos contained into the part (List shape only)
        /// </summary>
        /// <param name="part">The calendar part.</param>
        /// <param name="part">The page number.</param>
        /// <param name="part">The page size.</param>
        List<AggregatedEventViewModel> GetAggregatedList(CalendarPart part, int page, int pageSize);
    }
}