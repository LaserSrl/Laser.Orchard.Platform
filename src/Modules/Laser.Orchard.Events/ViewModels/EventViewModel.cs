using Orchard.ContentManagement;

namespace Laser.Orchard.Events.ViewModels
{
    // Il modello che viene utilizzato nella visualizzazione per lista
    public class EventViewModel
    {
        public IContent content { get; set; } 
        public string start { get; set; }
        public string end { get; set; }
    }
}