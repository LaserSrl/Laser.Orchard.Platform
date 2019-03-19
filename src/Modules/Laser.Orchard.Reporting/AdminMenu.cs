using Orchard.Localization;
using Orchard.Projections;
using Orchard.UI.Navigation;

namespace Laser.Orchard.Reporting {
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(menu => {
                menu.Caption(T("Charts & Reports"))
                    .Permission(Security.Permissions.ShowDataDashboard)
                    .Permission(Security.Permissions.ShowDataReports)
                    .Position("3.07");
                menu.Add(T("Query Report Definition"), "1.0", sub1 => sub1
                    .Action("Index", "Report", new { area = "Laser.Orchard.Reporting" })
                    .Permission(Permissions.ManageQueries));

                menu.Add(sub51 => sub51.Caption(T("Charts"))
                    .Permission(Security.Permissions.ShowDataReports)
                    .Action("ShowReports", "Report", new { area = "Laser.Orchard.Reporting" })
                );
                menu.Add(sub52 => sub52.Caption(T("Dashboards"))
                    .Permission(Security.Permissions.ShowDataDashboard)
                    .Action("DashboardList", "Report", new { area = "Laser.Orchard.Reporting" })
                );
            });
        }
    }
}