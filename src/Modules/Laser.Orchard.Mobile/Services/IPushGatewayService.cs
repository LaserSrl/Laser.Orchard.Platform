using Laser.Orchard.Mobile.Models;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Laser.Orchard.Mobile.Services {
    public interface IPushGatewayService : IDependency {

        IList GetPushQueryResult(Int32[] ids, bool countOnly = false, int contentId = 0);

        IList GetPushQueryResult(Int32[] ids, TipoDispositivo? tipodisp, bool produzione, string language, bool countOnly = false, ContentItem advItem = null);

        List<PushNotificationRecord> GetPushQueryResultByUserNames(string[] userNames, TipoDispositivo? tipodisp, bool produzione, string language);

        IList CountPushQueryResultByUserNames(string[] userNames, TipoDispositivo? tipodisp, bool produzione, string language);

        void PublishedPushEventTest(ContentItem ci);

        PushState PublishedPushEvent(ContentItem ci);

        IList<IDictionary> GetContactsWithDevice(string nameFilter);

        void SendPushToContact(ContentItem ci, string contactTitle);

        NotificationsCounters GetNotificationsCounters(ContentItem ci);
        void ResetNotificationFailures(ContentItem ci);
    }
}