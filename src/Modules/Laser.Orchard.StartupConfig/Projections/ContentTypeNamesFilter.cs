using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using OPServices = Orchard.Projections.Services;

namespace Laser.Orchard.StartupConfig.Projections {
    public class ContentTypeNamesFilter : OPServices.IFilterProvider {
        public ContentTypeNamesFilter() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeFilterContext describe) {
            describe.For("Content", T("Content"), T("Content"))
                .Element("ContentTypeNames", T("Content Type Names"),
                    T("Specific content types, specified by name and validated by a list of allowed content types."),
                    ApplyFilter,
                    DisplayFilter,
                    "ContentTypeNamesFilterForm"
                );
        }

        public void ApplyFilter(FilterContext context) {
            var contentTypeNames = (string)context.State.ContentTypeNames;
            var allowedContentTypes = (string)context.State.AllowedContentTypes;

            var contentTypesToFilter = new List<string>();
            var validFilter = GetContentTypesToFilter(context, out contentTypesToFilter);
            
            if (!validFilter) {
                // This happens when both parameters are compiled
                // but none of the specified content types are allowed
                // e.g.: ContentTypeNames: ContentType01, ContentType02, ContentType03
                // AllowedContentTypes: ContentType04
                // Search for a content item with negative Id
                context.Query = context.Query.Where(x => x.ContentItem(),
                    y => y.Lt("Id", 0));
            } else if (contentTypesToFilter.Any()) {
                context.Query = context.Query.ForType(contentTypesToFilter.ToArray());
            }
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Content with specified types that are allowed");
        }

        private bool GetContentTypesToFilter(FilterContext context, out List<string> cts) {
            var contentTypeNames = (string)context.State.ContentTypeNames;
            var allowedContentTypes = (string)context.State.AllowedContentTypes;

            var valid = true;
            cts = new List<string>();

            if (!string.IsNullOrEmpty(contentTypeNames)) {
                cts = contentTypeNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (!string.IsNullOrEmpty(allowedContentTypes)) {
                    var allowed = allowedContentTypes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    cts = cts.Where(s => allowed.Contains(s)).ToList();

                    valid = cts.Any();
                }
            } else {
                if (!string.IsNullOrEmpty(allowedContentTypes)) {
                    cts = allowedContentTypes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            }

            return valid;
        }
    }
}