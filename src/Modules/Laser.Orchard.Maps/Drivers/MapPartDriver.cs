using System;
using System.Xml.Linq;
using Laser.Orchard.Maps.Models;
using Laser.Orchard.Maps.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using System.Globalization;

namespace Laser.Orchard.Maps.Drivers {
    public class MapPartDriver : ContentPartCloningDriver<MapPart> {
        private readonly IOrchardServices _orchardServices;

        protected override string Prefix {
            get { return "Laser.Map"; }
        }
        public MapPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;

            //// test per cambio culture
            //System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;  // #GM 2015-09-15
        }
        public ILogger Logger { get; set; }

        public Localizer T { get; set; }

        protected override DriverResult Display(MapPart part, string displayType, dynamic shapeHelper) {
            var mapsSettings = _orchardServices.WorkContext.CurrentSite.As<MapsSiteSettingsPart>();

            if (displayType == "Summary")
                return ContentShape("Parts_Map_Summary",
                    () => shapeHelper.Parts_Map_Summary(Longitude: part.Longitude, Latitude: part.Latitude, LocationInfo: part.LocationInfo, LocationAddress: part.LocationAddress,
                        MapProvider: mapsSettings.MapsProvider,
                        MapTiles: mapsSettings.MapsTiles,
                        MaxZoom: mapsSettings.MaxZoom));
            if (displayType == "SummaryAdmin")
                return ContentShape("Parts_Map_SummaryAdmin",
                    () => shapeHelper.Parts_Map_Summary(Longitude: part.Longitude, Latitude: part.Latitude, LocationInfo: part.LocationInfo, LocationAddress: part.LocationAddress,
                        MapProvider: mapsSettings.MapsProvider,
                        MapTiles: mapsSettings.MapsTiles,
                        MaxZoom: mapsSettings.MaxZoom));

            if (mapsSettings.MapsProvider == MapsProviders.OpenStreetMap) {
                return ContentShape("Parts_OsmMap",
                    () => shapeHelper.Parts_OsmMap(Longitude: part.Longitude, Latitude: part.Latitude, LocationInfo: part.LocationInfo, LocationAddress: part.LocationAddress,
                        MapProvider: mapsSettings.MapsProvider,
                        MapTiles: mapsSettings.MapsTiles,
                        MaxZoom: mapsSettings.MaxZoom));
            } else {
                return ContentShape("Parts_Map",
                    () => shapeHelper.Parts_Map(Longitude: part.Longitude, Latitude: part.Latitude, LocationInfo: part.LocationInfo, LocationAddress: part.LocationAddress,
                        MapProvider: mapsSettings.MapsProvider,
                        MapTiles: mapsSettings.MapsTiles,
            MaxZoom: mapsSettings.MaxZoom));
            }
        }

        //GET
        protected override DriverResult Editor(MapPart part, dynamic shapeHelper) {
            var mapsSettings = _orchardServices.WorkContext.CurrentSite.As<MapsSiteSettingsPart>();
            var shapeName = "Parts_Map_Edit";
            var templateName = "Parts/Map";
            if (mapsSettings.MapsProvider == MapsProviders.OpenStreetMap) {
                shapeName = "Parts_OsmMap_Edit";
                templateName = "Parts/OsmMap";
            }
            var mapEdit = new MapEditModel {
                MapProvider = mapsSettings.MapsProvider,
                MapTiles = mapsSettings.MapsTiles,
                MaxZoom = mapsSettings.MaxZoom,
                GoogleApiKey = mapsSettings.GoogleApiKey,
                Map = part,
                DecimalSeparator = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator // #GM 2015-09-15
            };
            return ContentShape(shapeName,
                                () => shapeHelper.EditorTemplate(TemplateName: templateName,
                                    Model: mapEdit,
                                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(MapPart part, IUpdateModel updater, dynamic shapeHelper) {
            var partSettings = part.Settings.GetModel<MapPartSettings>();
            var mapEdit = new MapEditModel {
                Map = part
            };

            updater.TryUpdateModel(mapEdit, Prefix, null, null);
            if (partSettings.Required && (mapEdit.Map.Latitude + mapEdit.Map.Longitude) == 0) {
                updater.AddModelError("MapPartIsRequired", T("A point on the map is required."));
            }
            return Editor(part, shapeHelper);
        }

        #region [ Import/Export ]
        protected override void Exporting(MapPart part, ExportContentContext context) {

            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("Latitude", part.Latitude);
            root.SetAttributeValue("Longitude", part.Longitude);
            root.SetAttributeValue("LocationAddress", part.LocationAddress);
            root.SetAttributeValue("LocationInfo", part.LocationInfo);
        }

        protected override void Importing(MapPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            part.Latitude = Single.Parse(root.Attribute("Latitude").Value, CultureInfo.InvariantCulture);
            part.Longitude = Single.Parse(root.Attribute("Longitude").Value, CultureInfo.InvariantCulture);
            part.LocationAddress = root.Attribute("LocationAddress") != null ? root.Attribute("LocationAddress").Value : "";
            part.LocationInfo = root.Attribute("LocationInfo") != null ? root.Attribute("LocationInfo").Value : "";
        }
        #endregion

        protected override void Cloning(MapPart originalPart, MapPart clonePart, CloneContentContext context) {
            clonePart.Latitude = originalPart.Latitude;
            clonePart.Longitude = originalPart.Longitude;
            clonePart.LocationInfo = originalPart.LocationInfo;
            clonePart.LocationAddress = originalPart.LocationAddress;
        }
    }
}