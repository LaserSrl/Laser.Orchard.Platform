using Orchard.ContentManagement.Records;
using System;

namespace Laser.Orchard.CommunicationGateway.Models {

    public class CommunicationSmsRecord
    {
        public virtual int Id { get; set; }
        public virtual int SmsContactPartRecord_Id { get; set; }
        public virtual string Language { get; set; }
        public virtual bool Validated { get; set; }
        public virtual DateTime DataInserimento { get; set; }
        public virtual DateTime DataModifica { get; set; }
        public virtual string Sms { get; set; }
        public virtual string Prefix { get; set; }
        public virtual bool Produzione { get; set; }
        public virtual bool AccettatoUsoCommerciale { get; set; } //since we add these fields here, we need to add them to View_SmsVM_element for transfers
        public virtual bool AutorizzatoTerzeParti { get; set; } //we also need to update the OnSmsLoader() handler in CommunicationContactPartHandler.cs

        public CommunicationSmsRecord() {
            this.SmsContactPartRecord_Id = 0;
            this.Id = 0;
            this.Validated = true;
            this.DataInserimento = DateTime.Now;
            this.DataModifica = DateTime.Now;
            this.Produzione = true;
            this.AccettatoUsoCommerciale = false;
            this.AutorizzatoTerzeParti = false;
        }
    }
}