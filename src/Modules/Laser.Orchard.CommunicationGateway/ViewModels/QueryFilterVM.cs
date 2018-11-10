using System.Web.Mvc;

namespace Laser.Orchard.CommunicationGateway.ViewModels {

    public class QueryFilterVM {

        //  public string QueryTitle { get; set; }
        public string QueryId { get; set; }

        public SelectList ElencoQuery { get; set; }
    }
}