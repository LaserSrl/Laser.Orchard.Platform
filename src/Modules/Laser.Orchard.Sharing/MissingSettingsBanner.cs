using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using Laser.Orchard.Sharing.Models;

namespace Laser.Orchard.Sharing
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

            var shareSettings = _orchardServices.WorkContext.CurrentSite.As<ShareBarSettingsPart>();

            if (shareSettings == null || string.IsNullOrWhiteSpace(shareSettings.AddThisAccount))
            {
                yield return new NotifyEntry { Message = T("Content sharing settings needs to be configured."), Type = NotifyType.Warning };
            }
        }
    }
}
