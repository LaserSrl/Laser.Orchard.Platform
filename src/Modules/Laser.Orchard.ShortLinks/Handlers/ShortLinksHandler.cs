using Laser.Orchard.ShortLinks.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Autoroute.Services;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.UI.Notify;
using Orchard.Localization;

using Orchard;
using Orchard.Environment.Configuration;
using System.Web.Mvc;
using Laser.Orchard.ShortLinks.Services;

namespace Laser.Orchard.ShortLinks.Handlers {
    public class ShortLinksHandler : ContentHandler {
               
        private readonly INotifier _notifier;
        private readonly IShortLinksService _shortLinkService;
        public Localizer T { get; set; }

        public ShortLinksHandler(IRepository<ShortLinksRecord> repository, IShortLinksService shortLinkService, INotifier notifier) {
            Filters.Add(StorageFilter.For(repository));
            _shortLinkService = shortLinkService;
            T = NullLocalizer.Instance;
            
            _notifier = notifier;
            OnUpdated<ShortLinksPart>((context, part) => CheckAutoroute(context, part));
        }

        private void CheckAutoroute(UpdateContentContext context, ShortLinksPart part) {

            var fullAbsoluteUrl = _shortLinkService.GetFullAbsoluteUrl(part);

            if (!string.IsNullOrEmpty(part.FullLink) && part.FullLink != fullAbsoluteUrl) {
                _notifier.Warning(T("The short link is not associated to this page!"));
            }
        }   
    }
}