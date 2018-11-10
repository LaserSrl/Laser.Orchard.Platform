using Laser.Orchard.Commons.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Contrib.Widgets.Services {
    public class WidgetContainerPartDumperService : IDumperService {

        private readonly IWidgetManager _widgetManager;

        public WidgetContainerPartDumperService(
            IWidgetManager widgetManager) {

            _widgetManager = widgetManager;
        }

        public void DumpList(DumperServiceContext context) {
            var part = context.Content
                .ContentItem
                .Parts
                .FirstOrDefault(pa => pa.PartDefinition.Name == "WidgetsContainerPart");

            if (part != null) {
                var mainSb = new StringBuilder();
                mainSb.Append("{");
                mainSb.AppendFormat("\"n\": \"{0}\"", "WidgetList");
                mainSb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                mainSb.Append(", \"m\": [");

                var queryItems = _widgetManager.GetWidgets(part.Id);
                if (queryItems.Any()) {
                    mainSb.Append(
                        string.Join(",", queryItems
                            .Select((wep, index) => {
                                var sb = new StringBuilder();
                                sb.Append("{");
                                var dump = context.GetDumper()
                                    .Dump(wep, string.Format("[{0}]", index));
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