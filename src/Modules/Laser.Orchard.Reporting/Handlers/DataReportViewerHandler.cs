using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Laser.Orchard.Reporting.Models;

namespace Laser.Orchard.Reporting.Handlers {
    public class DataReportViewerHandler : ContentHandler
    {
        public DataReportViewerHandler(IRepository<DataReportViewerPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}