using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using Laser.Orchard.MailCommunication.Models;
using System.Web.Mvc;

namespace Laser.Orchard.MailCommunication.Services
{
    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class MissingSettingsBanner : INotificationProvider
    {
        private readonly IOrchardServices _orchardServices;

        public MissingSettingsBanner(IOrchardServices orchardServices)
        {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications()
        {
            var settingsMailer = _orchardServices.WorkContext.CurrentSite.As<MailerSiteSettingsPart>();
            if (settingsMailer == null || string.IsNullOrWhiteSpace(settingsMailer.FtpHost))
            {
                var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                var url = urlHelper.Action("Mailer", "Admin", new { Area = "Settings"});
                yield return new NotifyEntry { Message = T("The <a href=\"{0}\">Mailer</a> settings need to be configured.", url), Type = NotifyType.Warning };
            }

            var settingsMailCommunication = _orchardServices.WorkContext.CurrentSite.As<MailCommunicationSettingsPart>();
            if (settingsMailCommunication == null || settingsMailCommunication.IdTemplateUnsubscribe == null) 
            {
                var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                var url = urlHelper.Action("MailCommunication", "Admin", new { Area = "Settings" });
                yield return new NotifyEntry { Message = T("The <a href=\"{0}\">Mail Communication</a> settings need to be configured.", url), Type = NotifyType.Warning };
            }
        }
    }
}