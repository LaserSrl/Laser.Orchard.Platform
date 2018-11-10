using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Modules;
using Orchard.Modules.Services;
using Orchard.Modules.Models;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;

namespace Laser.Orchard.FidelitySimsol.Services
{
    public class NotificatorService : INotificationProvider
    {

        IEnumerable<FeatureDescriptor> features;

        public Localizer T { get; set; }

        //IModuleService modules
        public NotificatorService(IFeatureManager fmanager)
        {
            this.features = fmanager.GetEnabledFeatures();
        }


        public IEnumerable<NotifyEntry> GetNotifications()
        {
            IEnumerable<FeatureDescriptor> selecF = features.Where(f => f.Category != null && f.Category.Equals("FidelityProvider"));
            if (selecF.Count() == 0)
            {
                yield return new NotifyEntry { Message = T("There are no Module fidelity base enabled "), Type = NotifyType.Error };
            } 
        }
    }
}