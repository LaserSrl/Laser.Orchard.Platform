using Orchard.ContentManagement.Handlers;
using System.Collections.Generic;
using Laser.Orchard.StartupConfig.Models;
using Orchard.Tokens;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class DisplayTextPartHandler : ContentHandler {
        private readonly ITokenizer _tokenizer;
        public DisplayTextPartHandler(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
        }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.Parts.FirstOrDefault(p => p.PartDefinition.Name == "DisplayTextPart");
            if(part != null) {
                var settings = part.Settings.GetModel<DisplayTextPartSettings>();
                var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
                context.Metadata.DisplayText = _tokenizer.Replace(settings.DisplayText, tokens, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
            }
        }
    }
}