using System;

namespace Laser.Orchard.CommunicationGateway.Models {

    public class CommunicationEmailRecord {
        public virtual int Id { get; set; }
        public virtual int EmailContactPartRecord_Id { get; set; }
        public virtual string Language { get; set; }
        public virtual bool Validated { get; set; }
        public virtual DateTime DataInserimento { get; set; }
        public virtual DateTime DataModifica { get; set; }
        public virtual string Email { get; set; }
        public virtual bool Produzione { get; set; }
        public virtual bool AccettatoUsoCommerciale { get; set; } //since we add these fields here, we need to add them to View_EmailVM_element for transfers
        public virtual bool AutorizzatoTerzeParti { get; set; } //we also need to update the OnEmailLoader() handler in CommunicationContactPartHandler.cs
        public virtual string KeyUnsubscribe { get; set; }
        public virtual DateTime? DataUnsubscribe { get; set; }
    }
}