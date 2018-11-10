using System.Web;
using System.Web.Mvc;
using Laser.Orchard.NewsLetters.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Mvc.Extensions;

namespace Laser.Orchard.NewsLetters.Extensions {
    /// <summary>
    /// TODO: (PH:Autoroute) Many of these are or could be redundant (see controllers)
    /// </summary>
    public static class UrlHelperExtensions {


        #region [ Newsletter Definition ]
        public static string NewsLettersForAdmin(this UrlHelper urlHelper) {
            return urlHelper.Action("Index", "NewsLetterAdmin", new { area = "Laser.Orchard.NewsLetters" });
        }

        public static string NewsLetterCreate(this UrlHelper urlHelper) {
            return urlHelper.Action("Create", "NewsLetterAdmin", new { area = "Laser.Orchard.NewsLetters" });
        }

        public static string NewsLetterEdit(this UrlHelper urlHelper, NewsletterDefinitionPart newsletterDefinitionPart) {
            return urlHelper.Action("Edit", "NewsLetterAdmin", new { newsletterId = newsletterDefinitionPart.Id, area = "Laser.Orchard.NewsLetters" });
        }

        public static string NewsLetterRemove(this UrlHelper urlHelper, NewsletterDefinitionPart newsletterDefinitionPart) {
            return urlHelper.Action("Remove", "NewsLetterAdmin", new { newsletterId = newsletterDefinitionPart.Id, area = "Laser.Orchard.NewsLetters" });
        }
        #endregion

        #region [ Newsletter Subscribers ]
        public static string NewsLetterSubscribers(this UrlHelper urlHelper, NewsletterDefinitionPart newsletterDefinitionPart) {
            return urlHelper.Action("Index", "SubscribersAdmin", new { newsletterId = newsletterDefinitionPart.Id, area = "Laser.Orchard.NewsLetters" });
        }

        #endregion

        #region [ Newsletter Editions ]

        public static string NewsLetterEditionsForAdmin(this UrlHelper urlHelper, int newsletterDefinitionPartId) {
            return urlHelper.Action("Index", "NewsLetterEditionAdmin", new { newsletterId = newsletterDefinitionPartId, area = "Laser.Orchard.NewsLetters" });
        }
        public static string NewsLetterEditionCreate(this UrlHelper urlHelper, int newsletterDefinitionPartId) {
            return urlHelper.Action("Create", "NewsLetterEditionAdmin", new { newsletterId = newsletterDefinitionPartId, area = "Laser.Orchard.NewsLetters" });
        }
        public static string NewsLetterEditionEdit(this UrlHelper urlHelper, int newsletterDefinitionPartId, NewsletterEditionPart newsletterEditionPart) {
            return urlHelper.Action("Edit", "NewsLetterEditionAdmin", new { Id = newsletterEditionPart.Id, newsletterId = newsletterDefinitionPartId, area = "Laser.Orchard.NewsLetters" });
        }
        public static string NewsLetterEditionRemove(this UrlHelper urlHelper, NewsletterEditionPart newsletterEditionPart) {
            return urlHelper.Action("Remove", "NewsLetterEditionAdmin", new { newsletterEditionId = newsletterEditionPart.Id, area = "Laser.Orchard.NewsLetters" });
        }

        #endregion

        #region [ Subscribers ]
        public static string SubscriberRemove(this UrlHelper urlHelper, SubscriberRecord subscriberRecord) {
            return urlHelper.Action("Remove", "SubscribersAdmin", new { id = subscriberRecord.Id, area = "Laser.Orchard.NewsLetters" });
        }
        public static string SubscriptionSubscribe(this UrlHelper urlHelper) {
            return urlHelper.Action("Subscribe", "Subscription", new { area = "Laser.Orchard.NewsLetters" });
        }
        public static string SubscriptionConfirmSubscribe(this UrlHelper urlHelper) {
            return urlHelper.Action("ConfirmSubscribe", "Subscription", new { area = "Laser.Orchard.NewsLetters" });
        }
        public static string SubscriptionUnsubscribe(this UrlHelper urlHelper) {
            return urlHelper.Action("Unsubscribe", "Subscription", new { area = "Laser.Orchard.NewsLetters" });
        }
        public static string SubscriptionConfirmUnsubscribe(this UrlHelper urlHelper) {
            return urlHelper.Action("ConfirmUnsubscribe", "Subscription", new { area = "Laser.Orchard.NewsLetters" });
        }
        #endregion
    }
}