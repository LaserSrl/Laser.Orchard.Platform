using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;

namespace Laser.Orchard.Questionnaires.Models {

    public class RankingPart : ContentPart<RankingPartRecord> {

        public Int32 Point {
            get { return this.Retrieve(r => r.Point); }
            set { this.Store(r => r.Point, value); }
        }

        public string Identifier {
            get { return this.Retrieve(r => r.Identifier); }
            set { this.Store(r => r.Identifier, value); }
        }

        public string UsernameGameCenter {
            get { return this.Retrieve(r => r.UsernameGameCenter); }
            set { this.Store(r => r.UsernameGameCenter, value); }
        }

        public TipoDispositivo Device {
            get { return this.Retrieve(r => r.Device); }
            set { this.Store(r => r.Device, value); }
        }

        public Int32 ContentIdentifier {
            get { return this.Retrieve(r => r.ContentIdentifier); }
            set { this.Store(r => r.ContentIdentifier, value); }
        }

        public DateTime RegistrationDate {
            get { return this.Retrieve(r => r.RegistrationDate); }
            set { this.Store(r => r.RegistrationDate, value); }
        }

        public bool AccessSecured {
            get { return this.Retrieve(r => r.AccessSecured); }
            set { this.Store(r => r.AccessSecured, value); }
        }

        public Int32 User_Id {
            get { return this.Retrieve(r => r.User_Id); }
            set { this.Store(r => r.User_Id, value); }
        }
    }

    public class RankingPartRecord : ContentPartRecord {

        public virtual Int32 Point { get; set; }

        public virtual string Identifier { get; set; }

        public virtual string UsernameGameCenter { get; set; }

        public virtual TipoDispositivo Device { get; set; }

        public virtual Int32 ContentIdentifier { get; set; }

        public virtual DateTime RegistrationDate { get; set; }

        public virtual bool AccessSecured { get; set; }

        public virtual Int32 User_Id { get; set; }
    }

    public class RankingPartRecordIntermediate { //class used to extract RankingPartRecord from DB, since TIpoDispositivo is stored as a String there

        public virtual Int32 Point { get; set; }

        public virtual string Identifier { get; set; }

        public virtual string UsernameGameCenter { get; set; }

        public virtual string Device { get; set; }

        public virtual Int32 ContentIdentifier { get; set; }

        public virtual DateTime RegistrationDate { get; set; }

        public virtual bool AccessSecured { get; set; }

        public virtual Int32 User_Id { get; set; }
    }

    public enum TipoDispositivo { Android, Apple, WindowsMobile }

    public class RankingVM {

        public Int32 Point { get; set; }

        public string Identifier { get; set; }

        public string UsernameGameCenter { get; set; }

        public TipoDispositivo Device { get; set; }

        public Int32 ContentIdentifier { get; set; }
    }
}