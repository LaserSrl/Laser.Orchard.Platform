using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Laser.Orchard.StartupConfig.ShortCodes.Abstractions {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class Descriptor {
        public Descriptor(string name,
                   string signature,
                   LocalizedString buttonText,
                   LocalizedString description,
                   string shortCodeFormat,
                   string buttonIconClass ="",
                   EditorPage editor = null
            ) {
            Name = name;
            ButtonText = buttonText;
            Description = description;
            Signature = signature;
            ShortCodeFormat = shortCodeFormat;
            ButtonIconClass = buttonIconClass;
            Editor = editor;
        }
        public string Name { get; set; }
        public string Signature { get; set; }
        public LocalizedString ButtonText { get; set; }
        public string ButtonIconClass { get; set; }
        public LocalizedString Description { get; set; }
        public EditorPage Editor { get; set; }
        public string ShortCodeFormat { get; set; }

        public class EditorPage {
            public string ActionName { get; set; }
            public string ControllerName { get; set; }
            public RouteValueDictionary RouteValues { get; set; }
        }
    }
}