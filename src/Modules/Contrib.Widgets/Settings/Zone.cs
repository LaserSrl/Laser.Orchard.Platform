using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contrib.Widgets.Settings {
    public class Zone {
        public string ZoneName { get; set; }
        public List<Widget> Widgets { get; set; }
    }
}