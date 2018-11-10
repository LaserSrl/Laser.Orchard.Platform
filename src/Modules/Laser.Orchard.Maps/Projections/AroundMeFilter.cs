using Laser.Orchard.Maps.Models;
using Laser.Orchard.Maps.Services;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using System;
using System.Globalization;

namespace Laser.Orchard.Maps.Projections {

    public interface IFilterProvider : IEventHandler {

        void Describe(dynamic describe);
    }

    public class AroundMeFilter : IFilterProvider {
        private readonly IMapServices _mapServices;

        public AroundMeFilter(IMapServices mapServices) {
            T = NullLocalizer.Instance;
            _mapServices = mapServices;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            describe.For("Maps", T("Maps"), T("Maps"))
                .Element("AroundMe", T("Around Me"), T("Search for Content Items contained in an area"),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    "AroundMeForm"
                );
        }

        public void ApplyFilter(dynamic context) {
            var query = (IHqlQuery)context.Query;
            float lat = 0, lon = 0;
            int dist = -1;
            // try {
            if (context.State != null)
                if (context.State.Center != null && context.State.Center != "" && ((string)context.State.Center).Trim() != ",") {
                    try {
                        var points = ((string)context.State.Center).Split(',');
                        if (points.Length == 2) {
                            lat = float.Parse(points[0], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture);
                            lon = float.Parse(points[1], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture);
                            dist = int.Parse(context.State.Distance.ToString(), System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture);
                            if (dist < 0) {
                                throw new Exception(T("Validation Parameters: Latitude, Longitude, Dist not correct").ToString());
                            }
                        }
                        else {
                            throw new Exception(T("Validation Parameters: Latitude, Longitude, Dist not correct").ToString());
                        }
                    }
                    catch { throw new Exception(T("Validation Parameters: Latitude, Longitude, Dist not correct").ToString()); }
                    if ((lat != 0 || lon != 0) && dist >= 0) {
                        var conrners = _mapServices.GetTopRightAndBottomLeft(lat, lon, dist);
                        var minLat = Math.Min(conrners[1][0], conrners[0][0]);
                        var maxLat = Math.Max(conrners[1][0], conrners[0][0]);
                        var minLon = Math.Min(conrners[1][1], conrners[0][1]);
                        var maxLon = Math.Max(conrners[1][1], conrners[0][1]);

                        context.Query = query.Where(x => x.ContentPartRecord<MapVersionRecord>(), x => x.Between("Latitude", minLat, maxLat)).Where(x => x.ContentPartRecord<MapVersionRecord>(), x => x.Between("Longitude", minLon, maxLon));
                    }
                }
            //   }
            //  catch {  }
            return;
        }

        public LocalizedString DisplayFilter(dynamic context) {
            float lat = 0, lon = 0;
            int dist = -1;
            // try {
            if (context.State != null)
                if (context.State.Center != null && context.State.Center != "" && ((string)context.State.Center).Trim() != ",") {
                    try {
                        var points = ((string)context.State.Center).Split(',');
                        if (points.Length == 2) {
                            lat = float.Parse(points[0], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture);
                            lon = float.Parse(points[1], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture);
                            dist = int.Parse(context.State.Distance.ToString(), System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture);
                            if (dist > 0)
                                return T("Content Items contained in area with center {0} , {1} and distance {2}", lat, lon, dist);
                        }
                    }
                    catch { }
                }

            return T("Content Items contained in the specified area");
        }
    }
}