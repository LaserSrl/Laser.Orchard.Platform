using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;

namespace Laser.Orchard.Maps.Services {

    public interface IMapServices : IDependency {
        List<float[]> GetTopRightAndBottomLeft(double latitude, double longitude, int maxDistance);
        string CustomPinUrl(ContentItem content);
    }

    public class MapServices : IMapServices {
        private readonly ShellSettings _shellSettings;

        public MapServices(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
        }

        private double MToLat(double dyMeters, double aLat) {
            double rLat = aLat * Math.PI / 180;

            double m = 111132.09 * rLat - 566.05 * Math.Cos(2 * rLat) + 1.2 * Math.Cos(4 * rLat);
            double dLat = dyMeters / m;

            return dLat;
        }

        private double MToLon(double dxMeters, double aLon) {
            double rLon = aLon * Math.PI / 180;

            double m = 111415.13 * Math.Cos(rLon) - 94.55 * Math.Cos(3 * rLon);
            double dLon = dxMeters / m;

            return dLon;
        }

        public List<float[]> GetTopRightAndBottomLeft(double latitude, double longitude, int maxDistance) {
            double metersAroundMe = maxDistance;

            double dLat = MToLat(metersAroundMe, latitude);
            double dLon = MToLon(metersAroundMe, longitude);

            double trLat = latitude + dLat;
            double trLon = longitude + dLon;
            double blLat = latitude - dLat;
            double blLon = longitude - dLon;


            List<float[]> l = new List<float[]>();
            float[] top = { (float)trLat, (float)trLon };
            float[] bottom = { (float)blLat, (float)blLon };
            l.Add(top);
            l.Add(bottom);
            return l;
        }
        public string CustomPinUrl(ContentItem content) {
            return string.Format("~/Media/{0}/_mapspins/{1}/pin-{2}.png", _shellSettings.Name, content.ContentType.ToLower(), content.Id);
        }


    }
}