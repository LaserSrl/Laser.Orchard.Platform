using Orchard.Localization;

namespace Laser.Orchard.Reporting.ViewModels {
    public class ReportGroupByFieldViewModel
    {
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public string CategoryAndType { get; set; }
    }
}