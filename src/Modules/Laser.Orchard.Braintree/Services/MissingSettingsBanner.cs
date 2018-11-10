using Laser.Orchard.Braintree.Models;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;

namespace Laser.Orchard.Braintree.Services
{
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
            var settings = _orchardServices.WorkContext.CurrentSite.As<BraintreeSiteSettingsPart>();
            if (settings == null || string.IsNullOrWhiteSpace(settings.MerchantId))
            {
                var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                var url = urlHelper.Action("Index", "Admin", new { Area = "Laser.Orchard.Braintree" });
                yield return new NotifyEntry { Message = T("The <a href=\"{0}\">Braintree PayPal</a> settings need to be configured.", url), Type = NotifyType.Warning };
            }
        }
    }
}