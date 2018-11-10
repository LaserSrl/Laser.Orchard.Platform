using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Laser.Orchard.NewsLetters.ViewModels {
    public class SubscriberViewModel {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Guid { get; set; }
        public string Email { get; set; }
        public string LinkSubscription { get; set; }
        public string KeySubscription { get; set; }
        public string LinkUnsubscription { get; set; }
        public string KeyUnsubscription { get; set; }
        public DateTime SubscriptionDate { get; set; }

        public bool Confirmed { get; set; }
        public DateTime ConfirmationDate { get; set; }
        public DateTime UnsubscriptionDate { get; set; }
        public int NewsletterDefinition_Id { get; set; }
        public IContent NewsletterDefinition { get; set; }
        public int UserRecord_Id { get; set; }

    }
}