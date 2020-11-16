using Orchard.ContentManagement;
using Orchard;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.HiddenFields.Fields;
using Laser.Orchard.HiddenFields.Settings;
using Orchard.Tokens;

namespace Laser.Orchard.HiddenFields.Handlers {
    public class HiddenStringFieldHandler : ContentHandler {

        private readonly ITokenizer _tokenizer;

        public HiddenStringFieldHandler(ITokenizer tokenizer) {

            _tokenizer = tokenizer;

            OnUpdated<ContentPart>(InitializeFieldValue);
            OnCreated<ContentPart>(InitializeFieldValue);
        }

        private void InitializeFieldValue(ContentContextBase context, ContentPart part) {
            var fields = context.ContentItem.Parts
                    .SelectMany(pa => pa.Fields
                        .Where(fi => fi is HiddenStringField)
                        .Select(fi => fi as HiddenStringField));
            if (fields.Any()) {
                var tokens = new Dictionary<string, object> { { "Content", context.ContentItem } };

                foreach (var fi in fields) {
                    var settings = fi.PartFieldDefinition
                        .Settings.GetModel<HiddenStringFieldSettings>();
                    if (string.IsNullOrWhiteSpace(fi.Value) || settings.AutomaticAdjustmentOnEdit) {
                        fi.Value = settings.Tokenized ?
                            _tokenizer.Replace(settings.TemplateString, tokens) :
                            settings.TemplateString;
                    }
                }
            }
        }

    }
}