namespace Laser.Orchard.ContactForm.ViewModels
{
    public class ContactFormViewModel
    {
        public int ContentRecordId { get; set; }

        public bool ShowSubjectField { get; set; }

        public bool ShowNameField { get; set; }

        public bool RequireNameField { get; set; }

        public bool EnableFileUpload { get; set; }
    }
}