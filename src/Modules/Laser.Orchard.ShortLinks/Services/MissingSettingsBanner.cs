using Laser.Orchard.ShortLinks.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.ShortLinks.Services {

    public class MissingSettingsBanner : INotificationProvider {
        private readonly IOrchardServices _orchardServices;

        public MissingSettingsBanner(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            if (_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner)) {
                var settings = _orchardServices.WorkContext.CurrentSite.As<ShortLinksSettingsPart>();
                if (settings == null || string.IsNullOrWhiteSpace(settings.GoogleApiKey)) {
                    var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                    var url = urlHelper.Action("Index", "Admin", new { Area = "Settings", groupInfoId = "Index" });
                    yield return new NotifyEntry { Message = T("The <a href=\"{0}#shortlinksetting\">ShortLinks settings</a> need to be configured.", url), Type = NotifyType.Warning };
                }
            }
        }
    }
}