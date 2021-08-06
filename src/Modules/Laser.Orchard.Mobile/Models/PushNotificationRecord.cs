using System;

namespace Laser.Orchard.Mobile.Models {
    public class PushNotificationRecord {
        public virtual int Id { get; set; }
        public virtual TipoDispositivo Device { get; set; }
        public virtual string UUIdentifier { get; set; }
        public virtual string Token { get; set; }
        public virtual bool Validated { get; set; }
        public virtual DateTime DataInserimento { get; set; }
        public virtual DateTime DataModifica { get; set; }
        public virtual bool Produzione { get; set; }
        public virtual string Language { get; set; }
        public virtual int MobileContactPartRecord_Id { get; set; }
        public virtual string RegistrationUrlHost { get; set; }
        public virtual string RegistrationUrlPrefix { get; set; }
        public virtual string RegistrationMachineName { get; set; }
    }
    public enum TipoDispositivo { Android, Apple, AppleFCM, WindowsMobile }
}