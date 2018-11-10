using Laser.Orchard.Commons.Services;
using Orchard.ContentManagement;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Laser.Orchard.Generator.Services {
    public class ProjectionPartDumperService : IDumperService {

        private readonly IProjectionManager _projectionManager;

        public ProjectionPartDumperService(
            IProjectionManager projectionManager) {

            _projectionManager = projectionManager;
        }

        public void DumpList(DumperServiceContext context) {
            if (_projectionManager != null) {
                var part = context.Content
                .ContentItem
                .As<ProjectionPart>();

                if (part != null) {
                    var mainSb = new StringBuilder();

                    mainSb.Append("{");
                    mainSb.AppendFormat("\"n\": \"{0}\"", "ProjectionList");
                    mainSb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                    mainSb.Append(", \"m\": [");

                    var queryId = part.Record.QueryPartRecord.Id;
                    var queryItems = _projectionManager
                        .GetContentItems(queryId, (context.Page - 1) * context.PageSize, context.PageSize);

                    if (queryItems.Any()) {
                        mainSb.Append(
                            string.Join(",", queryItems
                                .Select((ci, index) => {
                                    var sb = new StringBuilder();
                                    sb.Append("{");
                                    var dump = context.GetDumper()
                                        .Dump(ci, string.Format("[{0}]", index));
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
}