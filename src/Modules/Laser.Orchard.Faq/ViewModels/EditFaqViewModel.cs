using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Laser.Orchard.Faq.Models;
using Orchard.Data.Conventions;

namespace Laser.Orchard.Faq.ViewModels
{
    public class EditFaqViewModel
    {
        [StringLengthMax]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Fill the required field 'Question'")]
        public string Question { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Fill the required field 'FAQ Type'")]
        public int FaqType { get; set; }
        public IEnumerable<FaqTypePartRecord> FaqTypes { get; set; }
        public IEnumerable<FaqPart> Faqs { get; set; }
    }
}