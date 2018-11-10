using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Mobile.Models {
    public class UserAgentRedirectPartRecord : ContentPartRecord {
        public UserAgentRedirectPartRecord() {
            Stores = new List<AppStoreRedirectRecord>();
        }
        public virtual bool AutoRedirect { get; set; }
        public virtual string AppName { get; set; }
        public virtual IList<AppStoreRedirectRecord> Stores { get; set; }
    }

    public class UserAgentRedirectPart : ContentPart<UserAgentRedirectPartRecord> {
        public bool AutoRedirect {
            get { return Record.AutoRedirect; }
            set { Record.AutoRedirect = value; }
        }
        public string AppName {
            get { return Record.AppName; }
            set { Record.AppName = value; }
        }
        public IEnumerable<AppStoreRedirectRecord> Stores {
            get { return Record.Stores.Select(s => s); }
        }
    }

    public class AppStoreRedirectRecord {
        public virtual int Id { get; set; }
        public virtual UserAgentRedirectPartRecord UserAgentRedirectPartRecord { get; set; }
        public virtual MobileAppStores AppStoreKey { get; set; }
        public virtual string RedirectUrl { get; set; }
    }


}