using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Localization.Records;

namespace Laser.Orchard.StartupConfig.Models {
    public class FavoriteCulturePart : ContentPart<FavoriteCulturePartRecord> {
        public int Culture_Id {
            get { return this.Retrieve<int>(x => x.Culture_Id); }
            set { this.Store<int>(x => x.Culture_Id, value); }
        }

        public string Culture { set; get; }
    }

    public class FavoriteCulturePartRecord : ContentPartVersionRecord {
        public virtual int Culture_Id { get; set; }
    }

}