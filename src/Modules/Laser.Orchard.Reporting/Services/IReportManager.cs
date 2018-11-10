using Orchard.ContentManagement;
using Orchard.Projections.Descriptors;
using Laser.Orchard.Reporting.Models;
using Laser.Orchard.Reporting.Providers;
using System.Collections.Generic;
using Orchard;
using Orchard.Security.Permissions;

namespace Laser.Orchard.Reporting.Services {
    public interface IReportManager: IDependency
    {
        IEnumerable<TypeDescriptor<GroupByDescriptor>> DescribeGroupByFields();
        IEnumerable<AggregationResult> RunReport(ReportRecord report, IContent container);
        IEnumerable<AggregationResult> RunHqlReport(ReportRecord report, IContent container, bool multiColumnTable = false);
        int GetCount(ReportRecord report, IContent container);
        IEnumerable<GenericItem> GetReportListForCurrentUser(string titleFilter = "");
        IEnumerable<GenericItem> GetDashboardListForCurrentUser(string titleFilter = "");
        IEnumerable<DataReportViewerPart> GetReports();
        Dictionary<int, Permission> GetReportPermissions();
        Dictionary<int, Permission> GetDashboardPermissions();
        string GetCsv(DataReportViewerPart part);
    }
}
