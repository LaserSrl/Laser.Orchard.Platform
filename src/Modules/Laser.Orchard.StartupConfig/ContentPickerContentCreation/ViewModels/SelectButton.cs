using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ContentPickerContentCreation.ViewModels {
    public class SelectButton {
        public string Callback { get; set; }

       public int IdContent { get; set; }

       public string NameCPFiels { get; set; }

       public string TitleContent { get; set; }

       public bool Published { get; set; }

    }
}