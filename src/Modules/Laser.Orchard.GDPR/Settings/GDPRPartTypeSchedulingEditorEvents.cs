using Laser.Orchard.GDPR.Extensions;
using Laser.Orchard.GDPR.Services;
using Laser.Orchard.GDPR.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.GDPR.Settings {
    [OrchardFeature("Laser.Orchard.GDPR.Scheduling")]
    public class GDPRPartTypeSchedulingEditorEvents : ContentDefinitionEditorEventsBase {

        private readonly IGDPRScheduleManager _GDPRScheduleManager;
        private readonly INotifier _notifier;

        public GDPRPartTypeSchedulingEditorEvents(
            IGDPRScheduleManager GDPRScheduleManager,
            INotifier notifier) {

            _GDPRScheduleManager = GDPRScheduleManager;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            // the settings make sense only for GDPRPart
            if (definition.PartDefinition.Name != "GDPRPart") {
                yield break;
            }

            var settings = definition
                .Settings
                .GetModel<GDPRPartTypeSchedulingSettings>()
                ?? new GDPRPartTypeSchedulingSettings(); //ensure that settings != null

            yield return DefinitionTemplate(MakeViewModel(settings));
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            // working on GDPRPart
            if (builder.Name != "GDPRPart") {
                yield break;
            }

            var vm = new GDPRPartTypeSchedulingSettingsViewModel();
            if (updateModel.TryUpdateModel(vm, "GDPRPartTypeSchedulingSettingsViewModel", null, null)) {
                var ok = true;
                if (vm.Settings.ScheduleAnonymization && vm.Settings.AnonymizationDaysToWait <= 0) {
                    // days must be positive
                    _notifier.Error(T("Days to wait before anonymization must be greater than zero. Scheduler configuration failed."));
                    ok = false;
                }
                if (vm.Settings.ScheduleErasure && vm.Settings.ErasureDaysToWait <= 0) {
                    // days must be positive
                    _notifier.Error(T("Days to wait before erasure must be greater than zero. Scheduler configuration failed."));
                    ok = false;
                }
                if (ok) {

                    var settings = vm.Settings;
                    GDPRPartTypeSchedulingSettings.SetValues(builder, settings);
                    // update scheduled tasks
                    _GDPRScheduleManager.UpdateSchedule(builder.TypeName, settings);
                }
            }
        }

        /// <summary>
        /// Create a ViewModel based on the settings provided.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private GDPRPartTypeSchedulingSettingsViewModel MakeViewModel(GDPRPartTypeSchedulingSettings settings) {
            return new GDPRPartTypeSchedulingSettingsViewModel {
                Settings = settings,
                AnonymizationEvents = ListEvents(settings),
                ErasureEvents = ListEvents(settings, true)
            };
        }

        private IEnumerable<SelectListItem> ListEvents(GDPRPartTypeSchedulingSettings settings, bool erasure = false) {
            foreach (var item in Enum.GetValues(typeof(EventForScheduling)).Cast<EventForScheduling>()) {
                yield return new SelectListItem {
                    Selected = item == (erasure ? settings.EventForErasure : settings.EventForAnonymization),
                    Text = FriendlyString(item),
                    Value = item.ToString()
                };
            }
        }

        private string FriendlyString(EventForScheduling efs) {
            switch (efs) {
                case EventForScheduling.Creation:
                    return T("Creation").Text;
                case EventForScheduling.LatestUpdate:
                    return T("Latest Update").Text;
                default:
                    return string.Empty;
            }
        }
        
    }
}