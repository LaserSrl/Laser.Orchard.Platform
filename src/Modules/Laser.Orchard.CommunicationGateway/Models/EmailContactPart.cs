using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;
using System.Collections.Generic;

namespace Laser.Orchard.CommunicationGateway.Models {

    public interface IEmailContactRecord : IContent {
        IList<CommunicationEmailRecord> EmailRecord { get; }
    }

    public class EmailContactPart : ContentPart<EmailContactPartRecord>, IEmailContactRecord {

        #region variabili usate per lazyload

        private readonly LazyField<IList<CommunicationEmailRecord>> _emailEntries = new LazyField<IList<CommunicationEmailRecord>>();
        public LazyField<IList<CommunicationEmailRecord>> EmailEntries { get { return _emailEntries; } }

        #endregion variabili usate per lazyload

        public IList<CommunicationEmailRecord> EmailRecord {
            get { return EmailEntries.Value; }
            set { EmailEntries.Value = value; }
        }
    }

    public class EmailContactPartRecord : ContentPartRecord {

        public EmailContactPartRecord() {
            EmailRecord = new List<CommunicationEmailRecord>();
        }

        public virtual IList<CommunicationEmailRecord> EmailRecord { get; set; }
    }
}