using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Laser.Orchard.ContactForm.Models;

namespace Laser.Orchard.ContactForm.Handlers
{
    public class ContactFormSettingsPartHandler : ContentHandler
    {
        public ContactFormSettingsPartHandler(IRepository<ContactFormSettingsRecord> repository) 
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<ContactFormSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) 
        {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Contact Form")));
        }
    }
}