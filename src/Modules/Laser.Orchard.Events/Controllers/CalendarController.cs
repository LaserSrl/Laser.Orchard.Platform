using Laser.Orchard.Events.Services;
using Laser.Orchard.Events.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.Events.Controllers
{
    public class CalendarController : Controller
    {
        private readonly IEventsService _eventsService;

        public CalendarController(IEventsService eventsService)
        {
            _eventsService = eventsService;
        }

        public ActionResult GetEvents(string start, string end, int queryId)
        {
            //Almeno una data tra quelle di inizio e fine intervallo deve essere valorizzata
            if (String.IsNullOrEmpty(start) && String.IsNullOrEmpty(end))
                return null;
            else
            {
                //Se solo una delle date è valorizzata imposto entrambi i limiti dell'intervallo di date a quella data
                DateTime intervalStart = !String.IsNullOrEmpty(start) ? DateTime.ParseExact(start, "yyyy-MM-dd", CultureInfo.InvariantCulture) : DateTime.ParseExact(end, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime intervalEnd = !String.IsNullOrEmpty(end) ? DateTime.ParseExact(end, "yyyy-MM-dd", CultureInfo.InvariantCulture) : DateTime.ParseExact(start, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                //Controllo che l'intervallo di date non sia troppo largo. Al momento il massimo è fisso a 18 mesi.
                int intervalMonths = (intervalEnd.Year - intervalStart.Year) * 12 + intervalEnd.Month - intervalStart.Month;
                if (intervalMonths > 18)
                    return null;
                else
                {
                    List<EventViewModel> list = _eventsService.GetEvents(queryId, intervalStart, intervalEnd, null, true);
                    List<JsonEventViewModel> listForCalendar = list.Select(s =>
                                                                            new JsonEventViewModel
                                                                            {
                                                                                id = ((dynamic)s).content.Id,
                                                                                title = ((dynamic)s).content.TitlePart.Title,
                                                                                start = ((dynamic)s).start,
                                                                                end = ((dynamic)s).end,
                                                                                allDay = ((dynamic)s).content.ActivityPart.AllDay
                                                                            }).ToList();

                    return Json(listForCalendar, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}