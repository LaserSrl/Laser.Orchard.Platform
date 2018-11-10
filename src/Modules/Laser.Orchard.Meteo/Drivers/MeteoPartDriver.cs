using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Meteo.Models;
using Orchard.ContentManagement.Drivers;

namespace Laser.Orchard.Meteo.Drivers {
    public class MeteoPartDriver :ContentPartDriver<MeteoPart> {
        protected override string Prefix {
            get {
                return "MeteoPart";
            }
        }
        protected override DriverResult Display(MeteoPart part, string displayType, dynamic shapeHelper) {
            if (displayType == "SummaryAdmin") {
                return ContentShape("Parts_Meteo_SummaryAdmin",
                        () => shapeHelper.Parts_Meteo_SummaryAdmin());
            }
            if (displayType == "Summary") {
                return ContentShape("Parts_Meteo_Summary",
                        () => shapeHelper.Parts_Meteo_Summary());
            }
            if (displayType == "Detail") {
                return ContentShape("Parts_Meteo",
                    () => shapeHelper.Parts_Meteo(Meteo: part));
            }
            return null;
        }
    }
}