
namespace Laser.Orchard.Events.ViewModels
{
    // Il modello che andrà trasformato nell'oggetto Json che fullcalendar usa come sorgente di eventi
    public class JsonEventViewModel
    {
        //Campi richiesti da fullcalendar
        //Una lista completa dei campi possibili si trova all'indirizzo http://fullcalendar.io/docs/event_data/Event_Object/
        public int id { get; set; }
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public bool allDay { get; set; }
    }
}