using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Faq.ViewModels
{
    public class ViewFaqViewModel
    {
        [Required]
        public string Question { get; set; }
        [Required]
        public string Answer { get; set; }
        public int Id { get; set; }
        public string FaqType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UrlToType { get; set; }

    }
}