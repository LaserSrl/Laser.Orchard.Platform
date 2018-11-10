using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.ViewModels {
    public class View_SmsVM {
        public List<View_SmsVM_element> Elenco { get; set; }
        public View_SmsVM() {
            Elenco = new List<View_SmsVM_element>();
        }
    }
    public class View_SmsVM_element {
        public int Id { get; set; }
        public int SmsContactPartRecord_Id { get; set; }
        public string Language { get; set; }
        public bool Validated { get; set; }
        public DateTime DataInserimento { get; set; }
        public DateTime DataModifica { get; set; }
        public string Sms { get; set; }
        public string Prefix { get; set; }
        public bool Produzione { get; set; }
        public bool Delete { get; set; }
        public bool AccettatoUsoCommerciale { get; set; }
        public bool AutorizzatoTerzeParti { get; set; }
        

        public View_SmsVM_element() {
            this.SmsContactPartRecord_Id = 0;
            this.Id = 0;
            this.Validated = true;
            this.DataInserimento = DateTime.Now;
            this.DataModifica = DateTime.Now;
            this.Produzione = true;
            this.Delete = false;
            this.AccettatoUsoCommerciale = false;
            this.AutorizzatoTerzeParti = false;
        }
    }
}