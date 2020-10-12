using Orchard.Security.Permissions;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.ContentManagement;
using Laser.Orchard.Reporting.Models;
using Orchard.Core.Title.Models;
using Orchard.Core.Common.Models;

namespace Laser.Orchard.Reporting.Security {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ShowAllDataReports = new Permission { Description = "Show All Data Reports on back-end menu", Name = "ShowAllDataReports" };
        public static readonly Permission ShowAllDataDashboard = new Permission { Description = "Show All Dashboards on back-end menu", Name = "ShowAllDataDashboard" };
        public static readonly Permission DownloadReportData = new Permission { Description = "Download report data", Name = "DownloadReportData" };
        public static readonly Permission DownloadDashboardData = new Permission { Description = "Download dashboard data", Name = "DownloadDashboardData" };
        public static readonly Permission ShowDataReports = new Permission { Description = "Show Data Reports on back-end menu", Name = "ShowDataReports", ImpliedBy = new[] { ShowAllDataReports, DownloadReportData } };
        public static readonly Permission ShowDataDashboard = new Permission { Description = "Show Dashboards on back-end menu", Name = "ShowDataDashboard", ImpliedBy = new[] { ShowAllDataDashboard, DownloadDashboardData } };
        private readonly IContentManager _contentManager;
        public Localizer T;
        public Feature Feature { get; set; }
        public Permissions(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ShowAllDataReports, ShowAllDataDashboard, DownloadReportData, DownloadDashboardData }
                },
                new PermissionStereotype {
                    Name = "Editor",
                },
                new PermissionStereotype {
                    Name = "Moderator",
                  },
                new PermissionStereotype {
                    Name = "Author",
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }

        public IEnumerable<Permission> GetPermissions() {
            var result = new List<Permission>();
            var reportPermissions = GetReportPermissions();
            result.Add(ShowDataReports);
            result.Add(ShowAllDataReports);
            result.Add(DownloadReportData);
            result.AddRange(reportPermissions.Values);
            var dashboardPermissions = GetDashboardPermissions();
            result.Add(ShowDataDashboard);
            result.Add(ShowAllDataDashboard);
            result.Add(DownloadDashboardData);
            result.AddRange(dashboardPermissions.Values);
            return result;
        }
        public Dictionary<int, Permission> GetReportPermissions() {
            Dictionary<int, Permission> result = new Dictionary<int, Permission>();
            // la seguente condizione where è necessaria per ragioni di performance
            var reportList = _contentManager.Query<DataReportViewerPart>().Where<DataReportViewerPartRecord>(x => true).List();
            foreach (var report in reportList) {
                var title = (report.ContentItem.Has<TitlePart>() ? report.ContentItem.As<TitlePart>().Title : T("[No Title]").ToString());
                result.Add(report.Id, new Permission {
                    Name = string.Format("ShowDataReport{0}", report.As<IdentityPart>().Identifier),
                    Description = string.Format("Show Data Report {0}", title),
                    ImpliedBy = new List<Permission>() { ShowAllDataReports }
                });
            }
            return result;
        }
        public Dictionary<int, Permission> GetDashboardPermissions() {
            Dictionary<int, Permission> result = new Dictionary<int, Permission>();
            var dashboardList = _contentManager.Query("DataReportDashboard").List();
            foreach (var dashboard in dashboardList) {
                var title = (dashboard.Has<TitlePart>() ? dashboard.As<TitlePart>().Title : T("[No Title]").ToString());
                result.Add(dashboard.Id, new Permission {
                    Name = string.Format("ShowDashboard{0}", dashboard.As<IdentityPart>().Identifier),
                    Description = string.Format("Show Dashboard {0}", title),
                    ImpliedBy = new List<Permission>() { ShowAllDataDashboard }
                });
            }
            return result;
        }
    }
}