using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using Laser.Orchard.Maps.Models.GoogleGeoCoding;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Projections.Models;
using Orchard.Projections.Services;

namespace Laser.Orchard.Maps.Controllers {

    [OrchardFeature("Laser.Orchard.Maps.Import")]
    public class OsmImportController : Controller {
        private readonly IContentManager _contentManager;
        private readonly IFieldIndexService _fieldIndexService;
        private readonly IRepository<DecimalFieldIndexRecord> _decimalFieldIndexRecord;
        private readonly IRepository<FieldIndexPartRecord> _fieldIndexRecord;
        private readonly Lazy<IAutorouteService> _autorouteService;

        ILogger Log { get; set; }
        public OsmImportController(IContentManager contentManager, IFieldIndexService fieldIndexService, IRepository<DecimalFieldIndexRecord> decimalFieldIndexRecord
            , IRepository<FieldIndexPartRecord> fieldIndexRecord, Lazy<IAutorouteService> autorouteService) {
            _contentManager = contentManager;
            _fieldIndexService = fieldIndexService;
            _decimalFieldIndexRecord = decimalFieldIndexRecord;
            _fieldIndexRecord = fieldIndexRecord;
            _autorouteService = autorouteService;

            Log = NullLogger.Instance;

        }

        //
        // GET: /OsmImport/
        public ActionResult Parkings(string osmFile, double latMin = 0, double lonMin = 0, double latMax = 0, double lonMax = 0) {

            object objOsm = DeserializzaDaFileSystemXmlReader(typeof(osm), Server.MapPath("~/Modules/Laser.Orchard.Maps/Contents/Osm/" + osmFile));

            osm osm = (osm)objOsm;

            int numParking = 0;
            int newParking = 0;
            int unnamedParkings = 0;
            //verificare se leggere anche le way
            foreach (node objnode in osm.node) {

                try {
                    if (objnode.tag != null) {
                        bool found = false;
                        string name = "ND";
                        int? capacita = null;
                        string address = "";
                        string locationInfo = "";
                        if ((objnode.lat >= latMin && objnode.lat <= latMax && objnode.lon >= lonMin && objnode.lon <= lonMax) || latMin + latMax + lonMin + lonMax == 0) {
                            foreach (tag objtag in objnode.tag) {

                                if (objtag.k.CompareTo("amenity") == 0 && objtag.v.CompareTo("parking") == 0) {
                                    /*
                                     <tag k='amenity' v='parking' />
                                    <tag k='name' v='Parcheggio Hotel beau Site' />
                                    <tag k='parking' v='surface' /><tag k='parking' v='underground' />
                                     <tag k='capacity' v='15' />
                                     <tag k='access' v='private' />
                             
                                     */
                                    //è un parcheggio, lo inserisco in db (o aggiorno quello esistente)
                                    found = true;
                                    numParking++;
                                }
                                if (objtag.k.CompareTo("name") == 0) {
                                    name = objtag.v;
                                }
                                if (objtag.k.CompareTo("capacity") == 0) {
                                    //trovato caso capacity = no pernotto legato ad un ristorante, valutare se leggere anche valori testuali (non ha senso per parcheggi)
                                    int temp;
                                    if (int.TryParse(objtag.v, out temp))
                                        capacita = temp;
                                }

                            }

                            if (found) {
                                var resp = MakeRequest(String.Format("https://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}", objnode.lat.ToString().Replace(",", "."), objnode.lon.ToString().Replace(",", ".")));
                                var geocodedName = resp.results.FirstOrDefault();
                                if (geocodedName != null) {
                                    if (name == "ND") {
                                        unnamedParkings++;
                                        name = geocodedName.formatted_address;
                                        locationInfo = "[null]";
                                    }
                                    address = geocodedName.formatted_address;
                                }
                                if (name == "ND") {
                                    name += " - " + objnode.id.ToString();
                                    locationInfo = "";
                                } else {
                                    locationInfo = locationInfo == "[null]" ? "" : name;
                                }
                                var parkContentId = 0;
                                parkContentId = _fieldIndexRecord.Fetch(
                                    f => f.DecimalFieldIndexRecords.Any<DecimalFieldIndexRecord>(
                                        r =>
                                            r.PropertyName == "Parcheggio.OsmId." &&
                                            r.Value == objnode.id
                                    )
                                ).Select(c => c.Id).FirstOrDefault();
                                ContentItem park = null;
                                park = _contentManager.Get(parkContentId);
                                if (park == null) {
                                    newParking++;
                                    park = _contentManager.New("Parcheggio");
                                    _contentManager.Create(park, VersionOptions.Latest);
                                }


                                ((dynamic)park).TitlePart.Title = name;
                                ((dynamic)park).MapPart.Longitude = (float)objnode.lon;
                                ((dynamic)park).MapPart.Latitude = (float)objnode.lat;
                                ((dynamic)park).MapPart.LocationInfo = locationInfo;
                                ((dynamic)park).MapPart.LocationAddress = address;

                                if (String.IsNullOrWhiteSpace(((dynamic)park).AutoroutePart.DisplayAlias)) {
                                    ((dynamic)park).AutoroutePart.DisplayAlias = _autorouteService.Value.GenerateAlias(((dynamic)park).AutoroutePart);
                                    ((dynamic)park).AutoroutePart.DisplayAlias = _autorouteService.Value.PublishAlias(((dynamic)park).AutoroutePart);
                                }

                                ((dynamic)park).Parcheggio.Capacity.Value = (Decimal)(capacita != null ? capacita : 0);
                                ((dynamic)park).Parcheggio.OsmId.Value = (Decimal)objnode.id;
                                ((dynamic)park).Parcheggio.OsmVersion.Value = (Decimal)objnode.version;
                                _fieldIndexService.Set(((dynamic)park).FieldIndexPart, "Parcheggio", "Capacity", "", (Decimal)(capacita != null ? capacita : 0), typeof(Decimal));
                                _fieldIndexService.Set(((dynamic)park).FieldIndexPart, "Parcheggio", "OsmId", "", (Decimal)objnode.id, typeof(Decimal));
                                _fieldIndexService.Set(((dynamic)park).FieldIndexPart, "Parcheggio", "OsmVersion", "", (Decimal)objnode.version, typeof(Decimal));
                                _contentManager.Publish(park);
                                //#if DEBUG
                                //                                if (numParking > 9) {
                                //                                    break;
                                //                                }
                                //#endif
                            }
                        }
                    }
                } catch (Exception ex2) {
                    Log.Error(ex2, "Object Node Id: " + objnode.id);

                    // LogErrore
                } finally {
                    osm = null;
                }
            }
            osm = null;
            var model = new ImportResult { UnnamedParkings = unnamedParkings, NewParkings = newParking, Parkings = numParking };
            return View(model);
        }


        private RootObject MakeRequest(string requestUrl) {
            try {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));

                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(RootObject));
                    object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                    RootObject jsonResponse
                    = objResponse as RootObject;
                    return jsonResponse;



                }
            } catch (Exception) {
                return null;
            }
        }



        private object DeserializzaDaFileSystemXmlReader(Type tipoPrincipale, string filename) {
            FileStream fs = null;
            try {

                // Create an instance of the XmlSerializer specifying type and namespace.
                XmlSerializer serializer = new XmlSerializer(tipoPrincipale);

                // A FileStream is needed to read the XML document.
                fs = new FileStream(filename, FileMode.Open);
                XmlReader reader = XmlReader.Create(fs);

                object objRet = serializer.Deserialize(reader);

                /* // Declare an object variable of the type to be deserialized.
                 OrderedItem i;

                 // Use the Deserialize method to restore the object's state.
                 i = (OrderedItem)serializer.Deserialize(reader);*/

                return objRet;
            } catch (Exception ex) {

                throw ex;
            } finally {
                if (fs != null)
                    fs.Close();
                fs.Dispose();
            }

        }

        [System.SerializableAttribute()]
        [XmlRootAttribute(IsNullable = false, ElementName = "osm")]
        public class osm {
            [XmlAttribute("version")]
            public string version;
            [XmlElement("node")]
            public node[] node;
            public osm() {
                //

                //

            }
        }

        public class node {

            [XmlAttribute("id")]
            public long id;
            [XmlAttribute("lat")]
            public double lat;
            [XmlAttribute("lon")]
            public double lon;
            [XmlAttribute("version")]
            public int version;


            [XmlElement("tag")]
            public tag[] tag;

            public node() {

            }

        }


        public class tag {

            [XmlAttribute("k")]
            public string k;
            [XmlAttribute("v")]
            public string v;

            public tag() {

            }

        }

        public class ImportResult {
            public int Parkings { get; set; }
            public int NewParkings { get; set; }
            public int UnnamedParkings { get; set; }
        }

    }
}