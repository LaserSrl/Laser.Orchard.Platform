using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Modules;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;

namespace Laser.Orchard.FidelityGateway.Services
{
    public class NotificatorService : INotificationProvider
    {

        IEnumerable<FeatureDescriptor> features;

        public Localizer T { get; set; }

        public NotificatorService(IFeatureManager fmanager)
        {
            this.features = fmanager.GetEnabledFeatures();
        }


        public IEnumerable<NotifyEntry> GetNotifications()
        {
            IEnumerable<FeatureDescriptor> selecF = features.Where(f => f.Category != null && f.Category.Equals("FidelityProvider"));
            if (selecF.Count() == 0)
            {
                yield return new NotifyEntry { Message = T("There are no Module fidelity provider enabled "), Type = NotifyType.Error };
            }
            if (selecF.Count() > 1)
            {
                string modul = selecF.OrderBy(a => a.Name).ToList()[0].Name;
                yield return new NotifyEntry { Message = T("There are too many Modules fidelity provider enabled. Module selected: " + modul), Type = NotifyType.Warning };
            }   
        }
    }
}