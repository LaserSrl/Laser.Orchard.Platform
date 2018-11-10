using Laser.Orchard.Events.Models;
using Laser.Orchard.Events.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System.Linq;
using System.Web.Routing;

namespace Laser.Orchard.Events.Handlers {
    public class CalendarPartHandler : ContentHandler {
        private readonly IEventsService _eventsService;
        private readonly RequestContext _requestContext;

        public CalendarPartHandler(IRepository<CalendarPartRecord> repository, IEventsService eventsService, RequestContext requestContext) {
            // Tell this handler to use CalendarPartRecord for storage.
            Filters.Add(StorageFilter.For(repository));

            _eventsService = eventsService;
            _requestContext = requestContext;

            OnLoaded<CalendarPart>((context, part) => LoadCalendar(context, part));
        }

        protected void LoadCalendar(LoadContentContext context, CalendarPart part) {
            //base.Loaded(context);

            if (_requestContext.HttpContext.Handler != null) {
                if (_requestContext.HttpContext.Request.RequestContext != null) //non-routed requests
               {
                    string usedController = _requestContext.HttpContext.Request.RequestContext.RouteData.Values["Controller"].ToString().ToLower();

                    if (usedController == "json") {
                        //if (context.ContentItem.Parts.SingleOrDefault(x => x.PartDefinition.Name == "CalendarPart") == null)
                        //    return;

                        //var calendarPart = (CalendarPart)context.ContentItem.Parts.SingleOrDefault(x => x.PartDefinition.Name == "CalendarPart");

                        part._eventList.Loader(() => {
                            return _eventsService.GetAggregatedList(part);
                        });
                    }
                }
            }
        }
    }
}