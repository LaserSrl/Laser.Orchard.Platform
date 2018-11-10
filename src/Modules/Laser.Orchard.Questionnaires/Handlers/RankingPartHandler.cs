using Laser.Orchard.Questionnaires.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Messaging.Events;
using Orchard.Messaging.Models;
using System.Collections.Generic;

namespace Laser.Orchard.Questionnaires.Handlers {

    public class RankingPartHandler : ContentHandler {

        public RankingPartHandler(IRepository<RankingPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }

    //public class MyMessageHandler : IMessageEventHandler {
    //    public void Sending(MessageContext context) {

    //        if (context.MessagePrepared)
    //            return;
    //        switch (context.Type) {
    //            case "ModuleRankingEmail":
    //                context.MailMessage.Subject = context.Properties["Subject"];
    //                context.MailMessage.Body = context.Properties["Body"];
    //                context.MessagePrepared = true;
    //                break;
    //        }
    //    }

    //    //we don't care about this right now 
    //    public void Sent(MessageContext context) { }
    //} 

}