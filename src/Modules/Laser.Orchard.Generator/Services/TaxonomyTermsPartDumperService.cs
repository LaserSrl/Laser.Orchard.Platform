using Laser.Orchard.Commons.Enums;
using Laser.Orchard.Commons.Services;
using Orchard.ContentManagement;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace Laser.Orchard.Generator.Services {
    public class TaxonomyTermsPartDumperService : IDumperService {

        private readonly ITaxonomyService _taxonomyService;

        public TaxonomyTermsPartDumperService(
            ITaxonomyService taxonomyService) {

            _taxonomyService = taxonomyService;
        }

        public void DumpList(DumperServiceContext context) {
            if (ShouldDoSomething(context.Content.ContentItem)) {
                var part = context.Content
                    .ContentItem
                    .As<TermPart>();

                if (part != null) {
                    var mainSb = new StringBuilder();
                    mainSb.Append("{");
                    Func<IContent, int, XElement> dumpFunc;
                    if (context.ResultTarget == ResultTarget.Contents) {
                        mainSb.AppendFormat("\"n\": \"{0}\"", "TaxonomyTermList");
                        mainSb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                        dumpFunc = (item, index) => context.GetDumper()
                            .Dump(item.ContentItem, string.Format("[{0}]", index));
                    } else {
                        mainSb.AppendFormat("\"n\": \"{0}\"", "TermPartList");
                        mainSb.AppendFormat(", \"v\": \"{0}\"", "TermPart[]");
                        dumpFunc = (item, index) => context.GetDumper()
                            .Dump(item, "TermPart");
                    }
                    mainSb.Append(", \"m\": [");

                    IEnumerable<IContent> termContentItems;
                    if (context.ResultTarget == ResultTarget.Terms) {
                        termContentItems = _taxonomyService
                            .GetChildren(part, true);
                    } else if (context.ResultTarget == ResultTarget.SubTerms) {
                        termContentItems = _taxonomyService
                            .GetChildren(part, false);
                    } else {
                        termContentItems = _taxonomyService
                            .GetContentItems(part, (context.Page - 1) * context.PageSize, context.PageSize);
                    }

                    if (termContentItems.Any()) {
                        mainSb.Append(
                            string.Join(",", termContentItems
                                .Select((ci, index) => {
                                    var sb = new StringBuilder();
                                    sb.Append("{");
                                    context.ConvertToJson(dumpFunc(ci, index), sb);
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

        private bool ShouldDoSomething(ContentItem contentItem) {
            return _taxonomyService != null
                && (contentItem.ContentType.EndsWith("Term")
                    || (
                        contentItem.TypeDefinition.Settings.ContainsKey("Taxonomy")
                        && !String.IsNullOrWhiteSpace(contentItem.TypeDefinition.Settings["Taxonomy"])
                    )
                );
        }

        private void GetItems(TermPart part, DumperServiceContext context) {

        }
    }
}