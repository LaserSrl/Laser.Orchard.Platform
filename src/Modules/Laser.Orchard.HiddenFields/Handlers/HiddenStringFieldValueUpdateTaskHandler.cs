using Laser.Orchard.HiddenFields.Fields;
using Laser.Orchard.HiddenFields.Services;
using Laser.Orchard.HiddenFields.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Logging;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Tasks;
using Orchard.Tasks.Scheduling;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.HiddenFields.Handlers {
    public class HiddenStringFieldValueUpdateTaskHandler : IScheduledTaskHandler {
        private readonly IContentManager _contentManager;
        private readonly IFieldIndexService _fieldIndexService;
        private readonly ITokenizer _tokenizer;
        private readonly ISweepGenerator _sweepGenerator;

        private static int PAGE_SIZE = 20;
        public static string UPDATEVALUES_TASK = "HiddenStringFieldValueUpdate";
        private IDictionary<HiddenStringFieldUpdateProcessVariant,
            Func<ContentPartFieldDefinitionBuilder, Func<ContentPart, bool>>> _partsEnumerableFilters;

        public ILogger Logger { get; set; }

        public HiddenStringFieldValueUpdateTaskHandler(
            IContentManager contentMananger,
            IFieldIndexService fieldIndexService,
            ITokenizer tokenizer,
            ISweepGenerator sweepGenerator) {

            _contentManager = contentMananger;
            _fieldIndexService = fieldIndexService;
            _tokenizer = tokenizer;
            _sweepGenerator = sweepGenerator;
            
            Logger = NullLogger.Instance;
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType.StartsWith(UPDATEVALUES_TASK + "_")) {
                _sweepGenerator.Terminate();
                try {
                    // Task name is something like HiddenStringFieldValueUpdate_ContentType_PartName_FieldName
                    var partFieldName = context.Task.TaskType.Split('_');
                    if (partFieldName.Length == 5) {
                        var typeName = partFieldName[1];
                        var partName = partFieldName[2];
                        var fieldName = partFieldName[3];
                        HiddenStringFieldUpdateProcessVariant updateType;
                        var validUpdate = Enum.TryParse(partFieldName[4], out updateType);
                        if (validUpdate) {
                            ProcessInternal(updateType, typeName, partName, fieldName);
                        }
                    }
                } catch (Exception ex) {
                    Logger.Error(ex, "Update Hidden String Values Error: " + context.Task.TaskType + " - " + ex.Message);
                } finally {
                    _sweepGenerator.Activate();
                }
            }
        }

        private void ProcessInternal(HiddenStringFieldUpdateProcessVariant variant, string contentTypeName, string partName, string fieldName) {
            var contents = _contentManager.Query(
                VersionOptions.Latest, contentTypeName);
            var contentCount = _contentManager.Query(
                VersionOptions.Latest, contentTypeName).Count();

            for (var i = 0; i < contentCount; i += PAGE_SIZE) {
                // Content Items with the required part.
                var contentPage = Enumerable.Empty<ContentItem>();
                if (variant == HiddenStringFieldUpdateProcessVariant.All) {
                    contentPage = contents.Slice(i, PAGE_SIZE)
                    .Where(ci => ci.Parts
                        .Any(pa => pa.PartDefinition.Name.Equals(partName, StringComparison.OrdinalIgnoreCase)));
                } else if (variant == HiddenStringFieldUpdateProcessVariant.Empty) {
                    contentPage = contents.Slice(i, PAGE_SIZE)
                    .Where(ci => ci.Parts
                        .Any(pa => pa.PartDefinition.Name.Equals(partName, StringComparison.OrdinalIgnoreCase) &&
                            string.IsNullOrWhiteSpace(
                                (pa.Fields
                                    .First(fi => fi.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)) as HiddenStringField).Value)));
                }
                
                // Read settings from the first content item
                var firstCi = contentPage.FirstOrDefault();
                if (firstCi != null) {
                    var field = firstCi.Parts
                        .SelectMany(p => p.Fields)
                        .FirstOrDefault(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                    if (field != null) {
                        var settings = field.PartFieldDefinition.Settings.GetModel<HiddenStringFieldSettings>();
                        if (settings != null) {
                            var parts = contentPage
                                .Select(ci => ci.Parts
                                    .FirstOrDefault(pa => pa.PartDefinition.Name.Equals(partName, StringComparison.OrdinalIgnoreCase)));
                            if (settings.Tokenized) {
                                ProcessPartsTokenized(fieldName, parts, settings.TemplateString);
                            } else {
                                ProcessParts(fieldName, parts, settings.TemplateString);
                            }
                        }
                    }
                }
            }
        }

        private void ProcessParts(string fieldName, IEnumerable<ContentPart> parts, string templateString) {
            foreach (var part in parts) {
                var fields = part.Fields
                    .Where(fi => fi is HiddenStringField && fi.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                    .Select(fi => fi as HiddenStringField);
                foreach (var field in fields) {
                    field.Value = templateString;
                    // Updates the index for projections
                    _fieldIndexService.Set(
                        part.ContentItem.As<FieldIndexPart>(),
                        part.PartDefinition.Name,
                        field.Name,
                        "",
                        field.Value,
                        typeof(string));
                }
            }
        }

        private void ProcessPartsTokenized(string fieldName, IEnumerable<ContentPart> parts, string templateString) {
            foreach (var part in parts) {
                var item = new KeyValuePair<HiddenStringField, Dictionary<string, object>>(
                    part.Fields.FirstOrDefault(fi => fi is HiddenStringField && fi.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)) as HiddenStringField,
                    new Dictionary<string, object> { { "Content", part.ContentItem } });
                item.Key.Value = _tokenizer.Replace(templateString, item.Value);
                // Updates the index for projections
                _fieldIndexService.Set(
                    part.ContentItem.As<FieldIndexPart>(),
                    part.PartDefinition.Name,
                    item.Key.Name,
                    "",
                    item.Key.Value,
                    typeof(string));
            }
        }
    }
}