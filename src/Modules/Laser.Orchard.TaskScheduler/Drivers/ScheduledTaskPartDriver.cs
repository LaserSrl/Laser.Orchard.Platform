using Laser.Orchard.TaskScheduler.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using System;
using System.Globalization;

namespace Laser.Orchard.TaskScheduler.Drivers {
    public class ScheduledTaskPartDriver : ContentPartDriver<ScheduledTaskPart> {
        private readonly IContentManager _contentManager;
        
        public ScheduledTaskPartDriver(IContentManager contentManager) { 
            _contentManager = contentManager;
        }
        
        protected override void Exporting(ScheduledTaskPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);

            root.SetAttributeValue("SignalName", part.SignalName);
            root.SetAttributeValue("ScheduledStartUTC", part.ScheduledStartUTC.HasValue ? part.ScheduledStartUTC.Value.ToString(CultureInfo.InvariantCulture) : null);
            root.SetAttributeValue("PeriodicityTime", part.PeriodicityTime.ToString());
            root.SetAttributeValue("PeriodicityUnit", part.PeriodicityUnit.ToString());
            // Export the identity of the content item
            var ci = _contentManager.Get(part.ContentItemId);
            if (ci != null) {
                var ciId = _contentManager.GetItemMetadata(ci).Identity.ToString();
                root.SetAttributeValue("ContentItemId", ciId);
            }
            // The task must not be set to be running, so RunningTaskId is set to 0.
            root.SetAttributeValue("RunningTaskId", 0);
            root.SetAttributeValue("Autodestroy", part.Autodestroy);
            root.SetAttributeValue("ExecutionType", part.ExecutionType.ToString());
            root.SetAttributeValue("LongTask", part.LongTask);
        }

        protected override void Importing(ScheduledTaskPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);

            if (root != null) {
                var signalName = root.Attribute("SignalName");
                if (signalName != null) {
                    part.SignalName = signalName.Value;
                }
                var scheduledStartUTC = root.Attribute("ScheduledStartUTC");
                if (scheduledStartUTC != null) {
                    // Since property (and related record property) ends with "UTC",
                    // the date has to be forced to DateTimeKind.Utc.
                    var utcDate = Convert.ToDateTime(scheduledStartUTC.Value, CultureInfo.InvariantCulture);
                    utcDate = new DateTime(utcDate.Year, utcDate.Month, utcDate.Day, 
                        utcDate.Hour,utcDate.Minute,utcDate.Second, DateTimeKind.Utc);
                    part.ScheduledStartUTC = utcDate;
                }
                var periodicityTime = root.Attribute("PeriodicityTime");
                if (periodicityTime != null) {
                    part.PeriodicityTime = Convert.ToInt32(periodicityTime.Value);
                }
                var periodicityUnit = root.Attribute("PeriodicityUnit");
                if (periodicityUnit != null) {
                    part.PeriodicityUnit = EnumExtension<TimeUnits>.ParseEnum(periodicityUnit.Value);
                }
                var contentItemId = root.Attribute("ContentItemId");
                if (contentItemId != null && !string.IsNullOrWhiteSpace(contentItemId.Value)) {
                    // Try to resolve identity and get the id on the database of the content item.
                    var ci = _contentManager.ResolveIdentity(new ContentIdentity(contentItemId.Value));
                    if (ci != null) {
                        part.ContentItemId = ci.Id;
                    }
                }
                // The task must not be set to be running, so RunningTaskId is set to 0.
                part.RunningTaskId = 0;
                var autodestroy = root.Attribute("Autodestroy");
                if (autodestroy != null) {
                    part.Autodestroy = Convert.ToBoolean(autodestroy.Value);
                }
                var executionType = root.Attribute("ExecutionType");
                if (executionType != null) {
                    part.ExecutionType = EnumExtension<ExecutionTypes>.ParseEnum(executionType.Value);
                }
                var longTask = root.Attribute("LongTask");
                if (longTask != null) {
                    part.LongTask = Convert.ToBoolean(longTask.Value);
                }
            }
        }
    }
}