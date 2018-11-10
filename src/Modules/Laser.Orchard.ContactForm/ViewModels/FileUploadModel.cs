using Orchard.MediaLibrary.Models;

namespace Laser.Orchard.ContactForm.ViewModels
{
    public class FileUploadModel
    {
        public int ParentID { get; set; }

        public MediaPart MediaData { get; set; }
    }
}