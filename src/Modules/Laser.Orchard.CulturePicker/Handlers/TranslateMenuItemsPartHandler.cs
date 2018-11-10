using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.CulturePicker.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Core.Common.Models;
using Orchard.Localization.Services;
using Orchard.ContentManagement;

namespace Laser.Orchard.CulturePicker.Handlers {
    [OrchardFeature("Laser.Orchard.CulturePicker.TranslateMenuItems")]
    public class TranslateMenuItemsPartHandler : ContentHandler {

        private readonly Services.ITranslateMenuItemsServices _translateMenuItemsService;

        public TranslateMenuItemsPartHandler(
            IRepository<TranslateMenuItemsPartRecord> repository,
            Services.ITranslateMenuItemsServices translateMenuItemsService) {
            
            Filters.Add(StorageFilter.For(repository));
            _translateMenuItemsService = translateMenuItemsService;
        }

        protected override void Published(PublishContentContext context) {
            var menu = ((dynamic)context.ContentItem); //the menu
            if (menu.ContentType == "Menu") {
                try {
                    var part = menu.TranslateMenuItemsPart;
                    if (!part.Translated && part.ToBeTranslated) {
                        //invoke the service to rebuild the menu
                        if (_translateMenuItemsService.TryTranslateAllSubmenus(part)) {
                            part.Translated = true;
                        }
                    }
                    part.ToBeTranslated = false;
                }
                catch { 

                }
            }
            
            
            base.Published(context);
        }


    }
}