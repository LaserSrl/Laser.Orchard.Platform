using Laser.Orchard.StartupConfig.ShortCodes.Abstractions;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Orchard.DisplayManagement;
using Orchard.Localization;
using System.Web.Routing;
using Laser.Orchard.StartupConfig.ShortCodes.Settings.Models;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.MetaData;
using Orchard;

namespace Laser.Orchard.StartupConfig.ShortCodes.Providers {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class ContentShortCodeProvider : IShortCodeProvider {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IShapeDisplay _shapeDisplay;
        private Descriptor _descriptor;
        public ContentShortCodeProvider(IOrchardServices orchardServices, IContentManager contentManager, IContentDefinitionManager contentDefinitionManager, IShapeDisplay shapeDisplay) {
            OrchardServices = orchardServices;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _shapeDisplay = shapeDisplay;
            T = NullLocalizer.Instance;
            _descriptor = new Descriptor("ContentPicker",
                "content",
                T("Content"),
                T("Adds a content as part of the text"),
                "[content id={0}]",
                "fa fa-file-text",
                new Descriptor.EditorPage { ActionName = "Index", ControllerName = "ContentShortCodeAdmin", RouteValues = new RouteValueDictionary(new { area = "Laser.Orchard.StartupConfig" }) });
        }

        public Localizer T { get; set; }
        public IOrchardServices OrchardServices { get; }

        public Descriptor Describe(DescribeContext context) {
            return _descriptor;
        }

        public void Evaluate(EvaluateContext context) {
            // TODO: Recursive content are a problem by now
            var source = context.SourceText;
            var pattern = @"\[" + _descriptor.Signature + @" [^\]]*id=(?<id>[0-9]*)[^\]]*\]";
            context.SourceText = Regex.Replace(source, pattern, ContentEvaluator, RegexOptions.Compiled);
        }

        private string ContentEvaluator(Match match) {
            string result = "";
            int contentId = 0;
            int.TryParse(match.Groups["id"].Value, out contentId);
            if (contentId > 0) {
                var content = _contentManager.Get(contentId);
                if (content != null) {
                    var shape = _contentManager.BuildDisplay(content);
                    shape.ShortCoded = true;
                    result = _shapeDisplay.Display(shape);
                }
            }
            return result;
        }
    }
}