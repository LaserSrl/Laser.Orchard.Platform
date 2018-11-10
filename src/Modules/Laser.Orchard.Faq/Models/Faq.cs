using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Laser.Orchard.Faq.Models
{
    public class FaqPartRecord : ContentPartRecord
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Fill the required field 'FAQ Type'")]
        public virtual int FaqTypeId { get; set; }
        [StringLengthMax]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Fill the required field 'Question'")]
        public virtual string Question { get; set; }
    }

    public class FaqPart : ContentPart<FaqPartRecord>
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Fill the required field 'FAQ Type'")]
        public int FaqTypeId
        {
            get { return Record.FaqTypeId; }
            set { Record.FaqTypeId = value; }
        }

        [StringLengthMax]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Fill the required field 'Question'")]
        public string Question
        {
            get { return Record.Question; }
            set { Record.Question = value; }
        }
    }
}