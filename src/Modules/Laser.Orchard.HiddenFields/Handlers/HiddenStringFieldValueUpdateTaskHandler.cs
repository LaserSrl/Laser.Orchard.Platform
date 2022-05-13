using Laser.Orchard.HiddenFields.Fields;
using Laser.Orchard.HiddenFields.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields.Handlers {
    public class HiddenStringFieldValueUpdateTaskHandler : IScheduledTaskHandler {
        private readonly IContentManager _contentManager;

        public static string UPDATEVALUES_TASK = "HiddenStringFieldValueUpdate";
        private IDictionary<HiddenStringFieldUpdateProcessVariant,
            Func<ContentPartFieldDefinitionBuilder, Func<ContentPart, bool>>> _partsEnumerableFilters;


        public HiddenStringFieldValueUpdateTaskHandler(
            IContentManager contentMananger) {
            _contentManager = contentMananger;

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

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType.StartsWith(UPDATEVALUES_TASK + "_")) {
                // Task name is something like HiddenStringFieldValueUpdate_ContentType_PartName_FieldName
                var partFieldName = context.Task.TaskType.Substring((UPDATEVALUES_TASK + "_").Length);

            }
        }
    }
}