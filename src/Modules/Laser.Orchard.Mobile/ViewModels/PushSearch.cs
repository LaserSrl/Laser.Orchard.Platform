
namespace Laser.Orchard.Mobile.ViewModels {
    public class PushSearch {
        public PushSearch() {
            Expression = "";
        }
        public string Expression { get; set; }
        public string SelectedMachineName { get; set; }
        public string ActualMachineName {
            get {
                return System.Environment.MachineName;
            }
        }
        public string Operation { get; set; }
    }
}


