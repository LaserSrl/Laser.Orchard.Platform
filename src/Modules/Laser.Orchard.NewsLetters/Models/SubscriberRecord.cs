using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NewsLetters.Models {
    public class SubscriberRecord {
        public SubscriberRecord() {
            ConfirmationDate = null;
            UnsubscriptionDate = null;
            Guid = System.Guid.NewGuid().ToString();
        }

        [Required]
        public virtual int Id { get; set; }
        [Required(ErrorMessage = "Name is required!")]
        public virtual string Name { get; set; }
        [Required]
        public virtual string Guid { get; set; }
        [Required(ErrorMessage = "A valid email is required!")]
        [DataType(DataType.EmailAddress, ErrorMessage = "A valid email is required!")]
        [RegularExpression("^[_a-zA-Z0-9-]+(.[a-zA-Z0-9-]+)@[a-zA-Z0-9-]+(.[a-zA-Z0-9-]+)*(.[a-zA-Z]{2,4})$", ErrorMessage = "A valid email is required!")]
        public virtual string Email { get; set; }
        [Required]
        public virtual DateTime SubscriptionDate { get; set; }

        public virtual bool Confirmed { get; set; }
        public virtual DateTime? ConfirmationDate { get; set; }
        public virtual DateTime? UnsubscriptionDate { get; set; }

        [Required]
        public virtual NewsletterDefinitionPartRecord NewsletterDefinition { get; set; }
        public virtual int UserRecord_Id { get; set; }

        public virtual string SubscriptionKey { get; set; }
        public virtual string UnsubscriptionKey { get; set; }
    }

}