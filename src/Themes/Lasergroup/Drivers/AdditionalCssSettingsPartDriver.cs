using Lasergroup.Models;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.CompilerServices;
using Orchard.ContentManagement;
using Lasergroup.ViewModels;
using Lasergroup.Extensions;

namespace Lasergroup.Drivers {
    public class AdditionalCssSettingsPartDriver : ContentPartDriver<AdditionalCssSettingsPart> {

        public AdditionalCssSettingsPartDriver() { }

        protected override string Prefix {
            get {
                return "AdditionalCssSettingsPart";
            }
        }
    }
}