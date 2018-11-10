using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;

namespace Laser.Orchard.CommunicationGateway.Models {

    public class CommunicationContactPart : ContentPart<CommunicationContactPartRecord> {

        public Int32 UserIdentifier {
            get { return this.Retrieve(r => r.UserPartRecord_Id); }
            set { this.Store(r => r.UserPartRecord_Id, value); }
        }

        /// <summary>
        /// Il ContentItem di tipo Master indica un contenuto creato da codice a cui sono legati tutti i contenuti senza legami, in questo modo rendo i contenuti querabili
        /// </summary>
        public bool Master {
            get { return this.Retrieve(r => r.Master); }
            set { this.Store(r => r.Master, value); }
        }

        /// <summary>
        /// Questo campo contiene annotazioni in sola lettura che vengono popolate dal sistema.
        /// </summary>
        public string Logs {
            get { return this.Retrieve(r => r.Logs); }
            set { this.Store(r => r.Logs, value); }
        }
    }

    public class CommunicationContactPartRecord : ContentPartRecord {
        public virtual Int32 UserPartRecord_Id { get; set; }
        public virtual bool Master { get; set; }
        public virtual string Logs { get; set; }
    }
}