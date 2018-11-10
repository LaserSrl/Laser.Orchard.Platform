using Orchard.ContentManagement;

namespace Laser.Orchard.ContactForm.Models {
    public class ContactFormSettingsPart : ContentPart<ContactFormSettingsRecord> {
        public bool EnableSpamProtection {
            get { return this.Retrieve(x => x.EnableSpamProtection); }
            set { this.Store(x => x.EnableSpamProtection, value); }

        }

        public bool EnableSpamEmail {
            get { return this.Retrieve(x => x.EnableSpamEmail); }
            set { this.Store(x => x.EnableSpamEmail, value); }
        }

        public string SpamEmail {
            get { return this.Retrieve(x => x.SpamEmail); }
            set { this.Store(x => x.SpamEmail, value); }
        }
    }
}