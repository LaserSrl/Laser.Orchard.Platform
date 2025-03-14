using Laser.Orchard.Mobile.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


namespace Laser.Orchard.Mobile.Services {
    [OrchardFeature("Laser.Orchard.Sms")]
    public class MissingSmsSettingsBanner : INotificationProvider {
        private readonly IOrchardServices _orchardServices;

        public MissingSmsSettingsBanner(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            var workContext = _orchardServices.WorkContext;
            var smsSettings = workContext.CurrentSite.As<SmsSettingsPart>();
            var context = new ValidationContext(smsSettings, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(smsSettings, context, results);

            if (smsSettings == null || !isValid) {
                var urlHelper = new UrlHelper(workContext.HttpContext.Request.RequestContext);
                var url = urlHelper.Action("SMS", "Admin", new { Area = "Settings" });
                yield return new NotifyEntry { Message = T("The <a href=\"{0}\">SMS settings</a> need to be configured.", url), Type = NotifyType.Warning };
            }
        }
    }

}
