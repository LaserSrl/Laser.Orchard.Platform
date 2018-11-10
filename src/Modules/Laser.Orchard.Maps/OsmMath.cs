using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Maps {
    public class OsmMath {
        public static int GetX(int zoom, double lon) {
            double z = (1 << zoom);
            double x = (((float)((lon + 180.0) / 360.0 * z)));
            return (int)x;
        }
        public static int GetY(int zoom, double lat) {
            double z = (1 << zoom);
            double y = (((float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * z)));
            return (int)y;
        }
    }
}