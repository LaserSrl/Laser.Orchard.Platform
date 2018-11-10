using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System.Xml.Serialization;
using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement.Utilities;


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