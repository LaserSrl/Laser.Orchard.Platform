using Laser.Orchard.Commons.Services;
using Laser.Orchard.Events.Models;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Laser.Orchard.Events.Services {
    public class CalendarPartDumperService : IDumperService {
        
        private readonly IEventsService _eventsService;

        public CalendarPartDumperService(
            IEventsService eventsService) {

            _eventsService = eventsService;
        }

        public void DumpList(DumperServiceContext context) {
            var part = context.Content
                .ContentItem
                .As<CalendarPart>();

            if (part != null) {
                var mainSb = new StringBuilder();
                mainSb.Append("{");
                mainSb.AppendFormat("\"n\": \"{0}\"", "EventList");
                mainSb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                mainSb.Append(", \"m\": [");

                var queryItems = _eventsService
                    .GetAggregatedList(part, context.Page, context.PageSize);
                if (queryItems.Any()) {
                    mainSb.Append(
                        string.Join(",", queryItems
                            .Select((aevm, index) => {
                                var sb = new StringBuilder();
                                sb.Append("{");
                                var dump = context.GetDumper()
                                    .Dump(aevm, string.Format("[{0}]", index));
                                context.ConvertToJson(dump, sb);
                                sb.Append("}");
                                return sb.ToString();
                            }))
                        );
                }

                mainSb.Append("]");
                mainSb.Append("}");

                // Add the serialization to the results
                context.ContentLists.Add(mainSb.ToString());
            }
        }

    }

}