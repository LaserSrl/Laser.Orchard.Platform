using Laser.Orchard.InsertStuff.Settings;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.InsertStuff.ViewModels {

    public class InsertStuffViewModel {
        public string DisplayName { get; set; }
        public List<string> StyleList { get; set; }
        public List<string> ScriptList { get; set; }
        public string RawHtml { get; set; }
        public bool OnFooter { get; set; }
    }
}