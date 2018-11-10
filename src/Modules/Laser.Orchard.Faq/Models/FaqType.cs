using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Faq.Models
{
    public class FaqTypePartRecord : ContentPartRecord
    {
        [StringLength(1024)]
        public virtual string Title { get; set; }
    }

    public class FaqTypePart : ContentPart<FaqTypePartRecord>
    {
        [Required(ErrorMessage = "Fill the required field 'Title'")]
        public string Title
        {
            get { return Record.Title; }
            set { Record.Title = value; }
        }
    }
}