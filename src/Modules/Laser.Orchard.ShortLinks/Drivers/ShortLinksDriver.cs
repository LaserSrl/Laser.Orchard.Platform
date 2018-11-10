using Laser.Orchard.ShortLinks.Models;
using Laser.Orchard.ShortLinks.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ShortLinks.Drivers {
    public class ShortLinksDriver: ContentPartDriver<ShortLinksPart> {

        private readonly IOrchardServices _service;
        private readonly IShortLinksService _shortLinkService;
        
        public ShortLinksDriver(IOrchardServices service, IShortLinksService shortLinkService) {
            _service = service;
            _shortLinkService = shortLinkService;
            
        }


        protected override DriverResult Display(ShortLinksPart part, string displayType, dynamic shapeHelper) {
            
            if (displayType == "SummaryAdmin") {
                return ContentShape("Parts_ShortLinks_SummaryAdmin", () => shapeHelper.Parts_ShortLinks_SummaryAdmin(
                    Url: part.Url));
            }
            return ContentShape("Parts_ShortLinks", () => shapeHelper.Parts_ShortLinks(
                Url: part.Url));
        
        }

        //GET
        protected override DriverResult Editor(ShortLinksPart part, dynamic shapeHelper) {
            return ContentShape("Parts_ShortLinks_Edit",
                 () => shapeHelper.EditorTemplate(
                     TemplateName: "Parts/ShortLinks",
                     Model: part,
                     Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(
            ShortLinksPart part, IUpdateModel updater, dynamic shapeHelper) {

            if (updater.TryUpdateModel(part, Prefix, null, null)) {

                if (string.IsNullOrEmpty(part.Url)) {
                    part.FullLink = string.Empty;
                } else if (string.IsNullOrEmpty(part.FullLink)) { // se sono qui è perchè l'utente ha scritto a mano lo short link e bisogna inserire il full link.
                    _shortLinkService.GetFullAbsoluteUrl(part);
                }
            }
           
            return Editor(part, shapeHelper);
        }


        protected override void Importing(ShortLinksPart part, ImportContentContext context) 
        {
            var importedUrl = context.Attribute(part.PartDefinition.Name, "Url");
            if (importedUrl != null) {
                part.Url = importedUrl;
            }

            var importedFullLink = context.Attribute(part.PartDefinition.Name, "FullLink");
            if (importedFullLink != null) {
                part.FullLink = importedFullLink;
            }
        }

        protected override void Exporting(ShortLinksPart part, ExportContentContext context) 
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Url", part.Url);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FullLink", part.FullLink);
        }


    }

}