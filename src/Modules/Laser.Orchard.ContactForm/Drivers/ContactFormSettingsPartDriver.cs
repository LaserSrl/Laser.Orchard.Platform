using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Laser.Orchard.ContactForm.Models;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.ContactForm.Drivers
{
    public class ContactFormSettingsPartDriver : ContentPartDriver<ContactFormSettingsPart> 
    {
        public ContactFormSettingsPartDriver() 
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "ContactFormSettings"; } }

        protected override DriverResult Editor(ContactFormSettingsPart part, dynamic shapeHelper)
        {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ContactFormSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {

            return ContentShape("Parts_ContactForm_SiteSettings", () =>
            {
                if (updater != null)
                {
                    updater.TryUpdateModel(part.Record, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: "Parts.ContactForm.SiteSettings", Model: part.Record, Prefix: Prefix);
            }).OnGroup("Contact Form");
        }

        #region [ Import/Export ]
        protected override void Exporting(ContactFormSettingsPart part, ExportContentContext context) {

            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("EnableSpamEmail", part.EnableSpamEmail);
            root.SetAttributeValue("EnableSpamProtection", part.EnableSpamProtection);
            root.SetAttributeValue("SpamEmail", part.SpamEmail);
        }

        protected override void Importing(ContactFormSettingsPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            if (root == null) {
                return;
            }
            part.EnableSpamEmail = bool.Parse(root.Attribute("EnableSpamEmail").Value);
            part.EnableSpamProtection = bool.Parse(root.Attribute("EnableSpamProtection").Value);
            part.SpamEmail = root.Attribute("SpamEmail").Value;
        }
        #endregion
    }
}