using Laser.Orchard.Mobile.Models;
using System;

namespace Laser.Orchard.Mobile.ViewModels {
    public class PushNotificationVM {
        public int Id { get; set; }
        public TipoDispositivo Device { get; set; }
        public string UUIdentifier { get; set; }
        public string Token { get; set; }
        public bool Validated { get; set; }
        public DateTime DataInserimento { get; set; }
        public DateTime DataModifica { get; set; }
        public bool Produzione { get; set; }
        public string Language { get; set; }
        public int MobileContactPartRecord_Id { get; set; }
        public string RegistrationUrlHost { get; set; }
        public string RegistrationUrlPrefix { get; set; }
        public string RegistrationMachineName { get; set; }
    }
}