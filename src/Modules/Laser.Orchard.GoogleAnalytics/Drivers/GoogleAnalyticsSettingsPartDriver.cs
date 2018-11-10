using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Laser.Orchard.GoogleAnalytics.Models;
using System;
using Orchard.UI.Notify;
using System.Web.Mvc;

namespace GoogleAnalytics.Drivers {
    public class GoogleAnalyticsSettingsPartDriver : ContentPartDriver<GoogleAnalyticsSettingsPart> {
        private const string TemplateName = "Parts/GoogleAnalyticsSettings";
        public Localizer T { get; set; }
        private readonly INotifier _notifier;

        public GoogleAnalyticsSettingsPartDriver(INotifier notifier) {
            T = NullLocalizer.Instance;
            _notifier = notifier;
        }


        protected override string Prefix { get { return "GoogleAnalyticsSettings"; } }

        //GET
        protected override DriverResult Editor(GoogleAnalyticsSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_GoogleAnalyticsSettings_Edit",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: TemplateName,
                        Model: part,
                        Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(GoogleAnalyticsSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            if (updater.TryUpdateModel(part, Prefix, null, null)) {
                if (string.IsNullOrWhiteSpace(part.GoogleAnalyticsKey) && (part.TrackOnFrontEnd || part.TrackOnAdmin)) {
                    updater.AddModelError("TrackingKeyMissing", T("If Google Analytics is enabled for the front end or the back end, the tracking key is required."));
                }
            } else {
                _notifier.Add(NotifyType.Error, T("Error on updating Google Analytics settings"));
            }
            return Editor(part, shapeHelper);
        }
    }
}