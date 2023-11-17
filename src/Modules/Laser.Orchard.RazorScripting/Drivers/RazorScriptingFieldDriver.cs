using Laser.Orchard.RazorScripting.Models;
using Laser.Orchard.RazorScripting.Settings;
using Laser.Orchard.StartupConfig.RazorCodeExecution.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.RazorScripting.Drivers {
    public class RazorScriptingFieldDriver : ContentFieldDriver<RazorScriptingField> {
        private readonly IRazorExecuteService _razorExecuteService;
        private readonly INotifier _notifier;

        public RazorScriptingFieldDriver(
            IRazorExecuteService razorExecuteServices,
            INotifier notifier) {

            _razorExecuteService = razorExecuteServices;
            _notifier = notifier;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private RazorScriptingFieldSettings GetSettings(RazorScriptingField field) {
            return field.PartFieldDefinition.Settings.GetModel<RazorScriptingFieldSettings>();
        }

        protected override DriverResult Display(ContentPart part, RazorScriptingField field, string displayType, dynamic shapeHelper) {
            var settings = GetSettings(field);
            if (settings != null) {
                var script = settings.DisplayScript;
                if (!string.IsNullOrWhiteSpace(script)) {
                    var ris = _razorExecuteService
                        .ExecuteString(script, part.ContentItem, null);
                    if (!string.IsNullOrWhiteSpace(ris)) {
                        Logger.Error(T("Error executing DisplayScript in {0}: {1}", 
                            field.Name, ris).Text);
                    }
                }
            }
            return null;
        }

        protected override DriverResult Editor(ContentPart part, RazorScriptingField field, dynamic shapeHelper) {
            var settings = GetSettings(field);
            if (settings != null) {
                var script = settings.GetEditorScript;
                if (!string.IsNullOrWhiteSpace(script)) {
                    var ris = _razorExecuteService
                        .ExecuteString(script, part.ContentItem, null);
                    if (!string.IsNullOrWhiteSpace(ris)) {
                        _notifier.Warning(T(ris));
                    }
                }
            }
            return null;
        }

        protected override DriverResult Editor(ContentPart part, RazorScriptingField field, IUpdateModel updater, dynamic shapeHelper) {
            var settings = GetSettings(field);
            if (settings != null) {
                var script = settings.PostEditorScript;
                if (!string.IsNullOrWhiteSpace(script)) {
                    var ris = _razorExecuteService
                        .ExecuteString(script, part.ContentItem, null);
                    if (!string.IsNullOrWhiteSpace(ris)) {
                        updater.AddModelError(field.Name, T(ris));
                    }
                }
            }
            // this returns null to prevent double execution of GET script
            return null;
        }
    }
}