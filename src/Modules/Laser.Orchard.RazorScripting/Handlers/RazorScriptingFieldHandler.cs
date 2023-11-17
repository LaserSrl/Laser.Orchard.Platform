using Laser.Orchard.RazorScripting.Models;
using Laser.Orchard.RazorScripting.Settings;
using Laser.Orchard.StartupConfig.RazorCodeExecution.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.RazorScripting.Handlers {
    public class RazorScriptingFieldHandler : ContentHandlerBase {

        // we are doing a ContentHandlerBase rather than a Driver to make
        // sure our methods are executed after those from Drivers.

        private readonly Lazy<IRazorExecuteService> _razorExecuteService;
        private readonly Lazy<INotifier> _notifier;

        public RazorScriptingFieldHandler(
            Lazy<IRazorExecuteService> razorExecuteService,
            Lazy<INotifier> notifier) {

            _razorExecuteService = razorExecuteService;
            _notifier = notifier;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public override void BuildDisplay(BuildDisplayContext context) {
            var fields = FieldsFromContext(context);
            foreach (var field in fields) {
                var result = ExecuteFieldScript(field, s => s.DisplayScript, context);
                if (!string.IsNullOrWhiteSpace(result)) {
                    Logger.Error(T("Error executing DisplayScript in {0}: {1}",
                        field.Name, result).Text);
                }
            }
        }

        public override void BuildEditor(BuildEditorContext context) {
            var fields = FieldsFromContext(context);
            foreach (var field in fields) {
                var result = ExecuteFieldScript(field, s => s.GetEditorScript, context);
                if (!string.IsNullOrWhiteSpace(result)) {
                    _notifier.Value.Warning(T(result));
                }
            }
        }

        public override void UpdateEditor(UpdateEditorContext context) {
            var fields = FieldsFromContext(context);
            foreach (var field in fields) {
                var result = ExecuteFieldScript(field, s => s.PostEditorScript, context);
                if (!string.IsNullOrWhiteSpace(result)) {
                    context.Updater.AddModelError(field.Name, T(result));
                }
            }
        }

        private string ExecuteFieldScript(
            RazorScriptingField field, Func<RazorScriptingFieldSettings, string> scriptGetter, BuildShapeContext context) {
            var settings = GetSettings(field);
            if (settings != null) {
                var script = scriptGetter(settings);
                if (!string.IsNullOrWhiteSpace(script)) {
                    return _razorExecuteService.Value
                        .ExecuteString(script, context.ContentItem, null);
                }
            }
            return null;
        }

        private IEnumerable<RazorScriptingField> FieldsFromContext(BuildShapeContext context) {
            return context.ContentItem
                .Parts
                .SelectMany(pa => pa.Fields.OfType<RazorScriptingField>());
        }

        private RazorScriptingFieldSettings GetSettings(RazorScriptingField field) {
            return field.PartFieldDefinition.Settings.GetModel<RazorScriptingFieldSettings>();
        }
    }
}