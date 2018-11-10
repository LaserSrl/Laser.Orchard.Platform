using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Laser.Orchard.Events.ViewModels
{
    public class Occurrence
    {
        public string Start { get; set; }
        public string End { get; set; }
    }

    public class AggregatedEventViewModel
    {
        public IContent Content { get; set; }
        public List<Occurrence> Occurrences { get; set; }
    }
}