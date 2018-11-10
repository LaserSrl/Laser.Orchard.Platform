using System.Web.Mvc;

namespace Laser.Orchard.Queries.Settings {

    public class QueryUserFilterExtensionPartSettingVM {
        public string QueryUserFilter { get; set; }
        public SelectList Elenco { get; set; }
        public string[] SelezionatiElenco { get; set; }
    }
}