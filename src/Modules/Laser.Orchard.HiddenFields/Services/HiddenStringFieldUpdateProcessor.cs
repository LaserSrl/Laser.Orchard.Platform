using Laser.Orchard.HiddenFields.Fields;
using Laser.Orchard.HiddenFields.Settings;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.State;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.HiddenFields.Services {
    public class HiddenStringFieldUpdateProcessor : IHiddenStringFieldUpdateProcessor {

        private readonly IContentManager _contentManager;
        private readonly IProcessingEngine _processingEngine;
        private readonly ShellSettings _shellSettings;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITokenizer _tokenizer;
        public Localizer T;

        private IDictionary<HiddenStringFieldUpdateProcessVariant, string> _variantDescriptions;
        private IDictionary<HiddenStringFieldUpdateProcessVariant, 
            Func<ContentPartFieldDefinitionBuilder, Func<ContentPart, bool>>> _partsEnumerableFilters;

        public HiddenStringFieldUpdateProcessor(
            IContentManager contentManager,
            IProcessingEngine processingEngine,
            ShellSettings shellSettings,
            IShellDescriptorManager shellDescriptionManager,
            IContentDefinitionManager contentDefinitionManager,
            ITokenizer tokenizer) {

            _contentManager = contentManager;
            _processingEngine = processingEngine;
            _shellSettings = shellSettings;
            _shellDescriptorManager = shellDescriptionManager;
            _contentDefinitionManager = contentDefinitionManager;
            _tokenizer = tokenizer;

            T = NullLocalizer.Instance;

            //dictionaries to handle task definitions
            _variantDescriptions = new Dictionary<HiddenStringFieldUpdateProcessVariant, string>() {
                { HiddenStringFieldUpdateProcessVariant.None, T("None").Text },
                { HiddenStringFieldUpdateProcessVariant.All, T("All fields").Text },
                { HiddenStringFieldUpdateProcessVariant.Empty, T("Empty fields").Text }
            };
            _partsEnumerableFilters = new Dictionary<HiddenStringFieldUpdateProcessVariant, 
                Func<ContentPartFieldDefinitionBuilder, Func<ContentPart, bool>>>() {
                    { HiddenStringFieldUpdateProcessVariant.None, (builder) => pa => false },
                    { HiddenStringFieldUpdateProcessVariant.All, (builder) => {
                        return pa => pa.PartDefinition.Name == builder.PartName;
                    }},
                    { HiddenStringFieldUpdateProcessVariant.Empty, (builder) => {
                        return pa => pa.PartDefinition.Name == builder.PartName &&
                            string.IsNullOrWhiteSpace(
                                (pa.Fields.First(fi => fi.Name == builder.Name) as HiddenStringField).Value);
                    } }
            };
        }

        private bool IsMyField(ContentField field, string name) {
            return field is HiddenStringField && field.Name == name;
        }

        public void Process(IEnumerable<int> contentItemIds, string partName, string fieldName, HiddenStringFieldSettings settings) {
            var parts = _contentManager.GetMany<ContentItem>(contentItemIds, VersionOptions.Published, new QueryHints())
                .Select(ci => ci.Parts.FirstOrDefault(pa => pa.PartDefinition.Name == partName));
            if (settings.Tokenized) {
                foreach (var item in parts.Select(pa => 
                    new KeyValuePair<HiddenStringField, Dictionary<string, object>>(
                        pa.Fields.FirstOrDefault(fi => IsMyField(fi, fieldName)) as HiddenStringField,
                        new Dictionary<string, object> { { "Content", pa.ContentItem } }
                        ))) {
                    item.Key.Value = _tokenizer.Replace(settings.TemplateString, item.Value);
                }
            } else {
                var fields = parts.SelectMany(pa => pa.Fields
                    .Where(fi => IsMyField(fi, fieldName))
                    .Select(fi => fi as HiddenStringField));
                foreach (var field in fields) {
                    field.Value = settings.TemplateString;
                }
            }
        }
        
        public IEnumerable<SelectListItem> GetVariants() {
            return _variantDescriptions.Select(kvp =>
                new SelectListItem {
                    Selected = false,
                    Text = kvp.Value,
                    Value = kvp.Key.ToString()
                });
        }

        public void AddTask(HiddenStringFieldUpdateProcessVariant variant, HiddenStringFieldSettings settings, ContentPartFieldDefinitionBuilder builder) {
            if (variant != HiddenStringFieldUpdateProcessVariant.None) {
                _processingEngine
                .AddTask(_shellSettings,
                    _shellDescriptorManager.GetShellDescriptor(),
                    "IHiddenStringFieldUpdateProcessor.Process",
                    new Dictionary<string, object> {
                        { "contentItemIds", GetIdsToProcess(variant, builder) },
                        { "partName", builder.PartName },
                        { "fieldName", builder.Name },
                        { "settings", settings }
                    });

            }
        }

        private IEnumerable<int> GetIdsToProcess(HiddenStringFieldUpdateProcessVariant variant, ContentPartFieldDefinitionBuilder builder) {
            var partDefinition = _contentDefinitionManager.GetPartDefinition(builder.PartName);

            return _contentManager.Query(
                _contentDefinitionManager.ListTypeDefinitions()
                    .Where(td => td.Parts.Any(ptd => ptd.PartDefinition.Name == partDefinition.Name))
                    .Select(td => td.Name)
                    .ToArray())
                .List()
                .Where(ci => ci.Parts.Where(_partsEnumerableFilters[variant](builder)).Any())
                .Select(ci => ci.Id);
        }
        
    }
}