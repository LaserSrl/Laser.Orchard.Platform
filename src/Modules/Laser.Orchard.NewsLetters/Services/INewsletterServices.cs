using System.Collections.Generic;
using Laser.Orchard.NewsLetters.Models;
using Laser.Orchard.NewsLetters.ViewModels;
using Orchard;
using Orchard.ContentManagement;

namespace Laser.Orchard.NewsLetters.Services {
    public interface INewsletterServices : IDependency {

        #region [ NewsletterDefinition ]
        void DeleteNewsletterDefinition(ContentItem newsletterDefinition);
        IEnumerable<NewsletterDefinitionPart> GetNewsletterDefinition();
        IEnumerable<NewsletterDefinitionPart> GetNewsletterDefinition(VersionOptions versionOptions);
        IEnumerable<NewsletterDefinitionPart> GetNewsletterDefinition(string ids, VersionOptions versionOptions);
        ContentItem GetNewsletterDefinition(int id, VersionOptions versionOptions);
        #endregion

        #region [ NewsletterEdition | Item]
        ContentItem GetNewsletterEdition(int id, VersionOptions versionOptions);
        IEnumerable<NewsletterEditionPart> GetNewsletterEditions(int newsletterId);
        IEnumerable<NewsletterEditionPart> GetNewsletterEditions(int newsletterId, VersionOptions versionOptions);
        void DeleteNewsletterEdition(ContentItem newsletterDefinition);
        IList<AnnouncementLookupViewModel> GetNextAnnouncements(int id, int[] selectedIds);
        IList<AnnouncementPart> GetAnnouncements(int[] selectedIds);
        void SendNewsletterEdition(ref NewsletterEditionPart newsletterEdition, string testEmail);
        #endregion

        #region [ Subscribers ]
        SubscriberRecord GetSubscriber(int id);
        IEnumerable<SubscriberRecord> GetSubscribers(int newsletterId);
        SubscriberRecord TryRegisterSubscriber(SubscriberViewModel subscriber);
        SubscriberRecord TryRegisterConfirmSubscriber(string keySubscribe);
        SubscriberRecord TryUnregisterSubscriber(SubscriberViewModel subscriber);
        SubscriberRecord TryUnregisterConfirmSubscriber(string keyUnsubscribe);

        #endregion
    }
}
