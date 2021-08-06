using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.ContentManagement.FieldStorage;
using Orchard.Mvc.Extensions;

namespace Orchard.Tokens.Providers {
    public class ContentDateModified : ITokenProvider {
        private readonly IContentManager _contentManager;

        public ContentDateModified(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Content", T("Content Items"), T("Content Items"))
                .Token("DateModified", T("Content Date Modified"), T("Date the content was created."), "DateTime")
                ;
        }

        public void Evaluate(EvaluateContext context) {
            context.For<IContent>("Content")
                .Token("DateModified", DateMod)
                .Chain("DateModified", "Date", DateMod)
                ;
        }

        private object DateMod(IContent content) {
            return content != null ? content.As<ICommonPart>().ModifiedUtc : null;
        }

    }
}
