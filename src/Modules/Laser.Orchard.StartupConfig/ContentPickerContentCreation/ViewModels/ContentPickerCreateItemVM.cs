using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.ContentPickerContentCreation.ViewModels {
    [OrchardFeature("Laser.Orchard.StartupConfig.ContentPickerContentCreation")]

    public class ContentPickerCreateItemVM {

        public List<String> contentTypeList { get; set; }

        public String nameCPField { get; set; }

        public bool multiple { get; set; }

    }
}