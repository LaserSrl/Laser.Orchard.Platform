using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Drivers {
    public class PerItemCachePartDriver : ContentPartDriver<PerItemCachePart> {


        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "PerItemKeyParam"; }
        }

        protected override DriverResult Editor(PerItemCachePart part, dynamic shapeHelper) {
            //check admin
            
            return ContentShape("Parts_PerItemCachePart_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts/PerItemCachePart_Edit", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PerItemCachePart part, IUpdateModel updater, dynamic shapeHelper) {

            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }



    }
}