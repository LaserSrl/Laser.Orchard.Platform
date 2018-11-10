using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Laser.Orchard.InsertStuff.Settings {

    public class InsertStuffFieldSettings {

        [AllowHtml]
        public string StyleList { get; set; }

        [AllowHtml]
        public string ScriptList { get; set; }

        [AllowHtml]
        public string RawHtml { get; set; }

        public bool OnFooter { get; set; }

        public InsertStuffFieldSettings() {
            OnFooter = true;
        }
    }
}