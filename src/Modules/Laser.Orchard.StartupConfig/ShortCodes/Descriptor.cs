using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Laser.Orchard.StartupConfig.ShortCodes {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class Descriptor {
        public Descriptor(string name,
                   string signature,
                   LocalizedString buttonText,
                   LocalizedString description,
                   string shortCodeFormat,
                   string buttonIconClass="",
                   EditorPage editor = null
            ) {
            Name = Name;
            ButtonText = buttonText;
            Description = description;
            Signature = signature;
            ShortCodeFormat = shortCodeFormat;
            ButtonIconClass = buttonIconClass;
            Editor = editor;
        }
        public string Name { get; }
        public string Signature { get; }
        public LocalizedString ButtonText { get; }
        public string ButtonIconClass { get; }
        public LocalizedString Description { get; }
        public EditorPage Editor { get; }
        public string ShortCodeFormat { get;  }

        public class EditorPage {
            public string ActionName { get; set; }
            public string ControllerName { get; set; }
            public RouteValueDictionary RouteValues { get; set; }
        }
    }
}