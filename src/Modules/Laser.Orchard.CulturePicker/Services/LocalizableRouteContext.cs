using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CulturePicker.Services {
    public class LocalizableRouteContext {
        public string QuerystringToLocalize { get; set; }
        public string UrlToLocalize { get; set; }
        public string Culture { get; set; }
        public string UrlLocalized { get; set; }
        public string QuerystringLocalized { get; set; }
    }
}