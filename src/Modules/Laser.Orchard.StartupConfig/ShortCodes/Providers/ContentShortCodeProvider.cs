using Laser.Orchard.StartupConfig.ShortCodes.Abstractions;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Orchard.DisplayManagement;

namespace Laser.Orchard.StartupConfig.ShortCodes.Providers {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class ContentShortCodeProvider : IShortCodeProvider {
        private readonly IContentManager _contentManager;
        private readonly IShapeDisplay _shapeDisplay;
        private Descriptor _descriptor;
        public ContentShortCodeProvider(IContentManager contentManager, IShapeDisplay shapeDisplay) {
            _contentManager = contentManager;
            _shapeDisplay = shapeDisplay;
            _descriptor = new Descriptor {
                Signature = "content"
            };
        }

        public Descriptor Describe() {
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
                    //TODO: Manage Alternates of children shapes too
                    //TODO: _ContentShortCode alternates should be more specific (placed at the end of the list)
                    shape.Metadata.Alternates.Add(content.ContentType + "_ContentShortCode");
                    result = _shapeDisplay.Display(shape);
                }
            }
            return result;
        }
    }
}