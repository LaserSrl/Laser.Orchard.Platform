using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;
using System.Collections.Generic;


namespace Laser.Orchard.Mobile.Models {
    public interface IMobileRecord : IContent {
        IList<PushNotificationRecord> MobileRecord { get; }
    }
    public class MobileContactPart : ContentPart<MobileContactPartRecord>, IMobileRecord {
        #region variabili usate per lazyload
        private readonly LazyField<IList<PushNotificationRecord>> _mobileEntries = new LazyField<IList<PushNotificationRecord>>();
        public LazyField<IList<PushNotificationRecord>> MobileEntries { get { return _mobileEntries; } }
        #endregion

        public IList<PushNotificationRecord> MobileRecord {
            get { return MobileEntries.Value; }
            set { MobileEntries.Value = value; }
        }
    }



    public class MobileContactPartRecord : ContentPartRecord {
        public MobileContactPartRecord() {
            MobileRecord = new List<PushNotificationRecord>();
        }

        public virtual IList<PushNotificationRecord> MobileRecord { get; set; }
    }
}