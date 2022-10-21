using Orchard.ContentManagement.Handlers;
using System.Collections.Generic;
using Laser.Orchard.StartupConfig.Models;
using Orchard.Tokens;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class DisplayTextPartHandler : ContentHandler {
        private readonly ITokenizer _tokenizer;
        private Dictionary<int,string> _displayText;
        public DisplayTextPartHandler(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
            _displayText = new Dictionary<int, string>();
        }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.Parts.FirstOrDefault(p => p.PartDefinition.Name == "DisplayTextPart");
            if (part != null) {
                var settings = part.Settings.GetModel<DisplayTextPartSettings>();
                var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
                if (!_displayText.ContainsKey(part.ContentItem.Id)) {
                    _displayText.Add(part.ContentItem.Id, _tokenizer.Replace(settings.DisplayText, tokens, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode })) ;
                }
                context.Metadata.DisplayText = _displayText[part.ContentItem.Id];
            }
        }
    }
}