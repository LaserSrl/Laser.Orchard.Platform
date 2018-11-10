using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.NewsLetters.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.NewsLetters.Drivers {
    public class AnnouncementPartDriver : ContentPartDriver<AnnouncementPart> {
        public AnnouncementPartDriver() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "NewsLetters"; }
        }

        protected override DriverResult Display(AnnouncementPart part, string displayType, dynamic shapeHelper) {
            if (displayType == "SummaryAdmin") {
                return ContentShape("Parts_Announcement_SummaryAdmin",
                        () => shapeHelper.Parts_NewsletterDefinition_SummaryAdmin());
            }
            if (displayType == "Summary") {
                return ContentShape("Parts_Announcement_Summary",
                        () => shapeHelper.Parts_NewsletterDefinition_Summary());
            }
            if (displayType == "Detail") {
                return ContentShape("Parts_Announcement",
                        () => shapeHelper.Parts_NewsletterDefinition());
            }
            return null;
        }
        protected override DriverResult Editor(AnnouncementPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Announcement_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts/Announcement_Edit",
                                 Model: part,
                                 Prefix: Prefix));
        }

        protected override DriverResult Editor(AnnouncementPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!updater.TryUpdateModel(part, Prefix, null, null)) {
                updater.AddModelError("AnnouncementPartError", T("Announcement Error"));
            }
            var selectedNews = HttpContext.Current.Request.Form[Prefix + ".AnnouncementPart_Newsletters_Ids"];
            selectedNews = String.Join(",", selectedNews.Split(',').Where(w => w != "false"));
            part.AttachToNextNewsletterIds = selectedNews;
            return Editor(part, shapeHelper);
        }


        //#region [ Import/Export ]
        //protected override void Exporting(AnnouncementPart part, ExportContentContext context) {
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("AnnouncementTitle", part.AnnouncementTitle);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("AttachToNextNewsletterIds", part.AttachToNextNewsletterIds);
        //}

        //protected override void Importing(AnnouncementPart part, ImportContentContext context) {
        //    var importedAnnouncementTitle = context.Attribute(part.PartDefinition.Name, "AnnouncementTitle");
        //    if (importedAnnouncementTitle != null) {
        //        part.AnnouncementTitle = importedAnnouncementTitle;
        //    }

        //    var importedAttachToNextNewsletterIds = context.Attribute(part.PartDefinition.Name, "AttachToNextNewsletterIds");
        //    if (importedAttachToNextNewsletterIds != null) {
        //        part.AttachToNextNewsletterIds = importedAttachToNextNewsletterIds;
        //    }

        //}
        //#endregion

    }
}