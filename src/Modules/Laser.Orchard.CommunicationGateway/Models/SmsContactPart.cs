using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;
using System.Collections.Generic;

namespace Laser.Orchard.CommunicationGateway.Models {

    public interface ISmsContactRecord : IContent {
        IList<CommunicationSmsRecord> SmsRecord { get; }
    }

    public class SmsContactPart : ContentPart<SmsContactPartRecord>, ISmsContactRecord {

        #region variabili usate per lazyload

        private readonly LazyField<IList<CommunicationSmsRecord>> _smsEntries = new LazyField<IList<CommunicationSmsRecord>>();
        public LazyField<IList<CommunicationSmsRecord>> SmsEntries { get { return _smsEntries; } }

        #endregion variabili usate per lazyload

        public IList<CommunicationSmsRecord> SmsRecord {
            get { return SmsEntries.Value; }
            set { SmsEntries.Value = value; }
        }
    }

    public class SmsContactPartRecord : ContentPartRecord {

        public SmsContactPartRecord() {
            SmsRecord = new List<CommunicationSmsRecord>();
        }

        public virtual IList<CommunicationSmsRecord> SmsRecord { get; set; }
    }
}