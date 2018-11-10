using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Laser.Orchard.WebServices.Controllers {
    public class GiroParchiController : Controller {

        private const string baseURL = "http://www.giroparchi.it/";
        private Dictionary<string, Dictionary<string, int>> POITermsDictionary = new Dictionary<string, Dictionary<string, int>>();
        private Dictionary<string, Dictionary<int, int>> TracksTermsDictionary = new Dictionary<string, Dictionary<int, int>>();
        private Dictionary<string, Dictionary<string, int>> TerrainTypeTermsDictionary = new Dictionary<string, Dictionary<string, int>>();

        private readonly IContentManager _contentManager;
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ILocalizationService _localizationService;

        public GiroParchiController(IContentManager contentManager, IMediaLibraryService mediaLibraryService, ITaxonomyService taxonomyService, ILocalizationService localizationService) {
            _contentManager = contentManager;
            _mediaLibraryService = mediaLibraryService;
            _taxonomyService = taxonomyService;
            _localizationService = localizationService;
        }

        private ActionResult ImportPOI() {

            HttpWebRequest request;
            HttpWebResponse response;

            string poi_tree_result;
            Dictionary<dynamic, dynamic> POITreeDictionary = new Dictionary<dynamic, dynamic>();

            POITermsDictionary.Add("it",
                                new Dictionary<string, int> {
                                        { "altre attrattive", 89 },
                                        { "beni culturali", 32 },
                                        { "centri storici", 30 },
                                        { "edifici religiosi", 31 },
                                        { "parchi e natura", 90 }
                                                            });

            POITermsDictionary.Add("fr",
                    new Dictionary<string, int> {
                                        { "altre attrattive", 93 },
                                        { "beni culturali", 41 },
                                        { "centri storici", 40 },
                                        { "edifici religiosi", 42 },
                                        { "parchi e natura", 94 }
                                                            });

            POITermsDictionary.Add("en",
                new Dictionary<string, int> {
                                        { "altre attrattive", 116 },
                                        { "beni culturali", 117 },
                                        { "centri storici", 118 },
                                        { "edifici religiosi", 119 },
                                        { "parchi e natura", 120 }
                                                            });

            try {

                string URL = baseURL + "RPC2";
                request = (HttpWebRequest)WebRequest.Create(URL);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";

                using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
                    string json = "{\"id\":\"djangorpc\", \"method\" : \"resources.get_poicategory_visible_tree\", \"params\" : [\"it\"]}";

                    streamWriter.Write(json);
                    streamWriter.Flush();
                }

                response = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(response.GetResponseStream())) {
                    poi_tree_result = streamReader.ReadToEnd();
                    POITreeDictionary = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(poi_tree_result);
                }

            } catch (Exception e) {
                poi_tree_result = e.Message;
            }

            dynamic model = new ExpandoObject();
            model.TreeResult = poi_tree_result;
            model.ImportResult = "";

            try {
                if (!String.IsNullOrWhiteSpace(POITreeDictionary["result"].ToString())) {

                    Newtonsoft.Json.Linq.JArray category = POITreeDictionary["result"];

                    foreach (Newtonsoft.Json.Linq.JToken item in category) {

                        string parentCategory = item["name"].ToString().ToLower();

                        bool hasChildren = false;

                        foreach (Newtonsoft.Json.Linq.JToken child in item["children"]) {
                            hasChildren = true;
                            model.ImportResult += importCategory(child, parentCategory) + "\n";
                        }

                        if (!hasChildren)
                            model.ImportResult += importCategory(item, parentCategory) + "\n";
                    }
                }
            } catch (Exception e) {
                model.ImportResult = e.Message;
            }

            return View("Import", model);
        }

        private string importCategory(Newtonsoft.Json.Linq.JToken item, string category) {

            string category_result;
            string poi_result = "";
            string import_result = "";

            Dictionary<dynamic, dynamic> CategoryDictionary = new Dictionary<dynamic, dynamic>();
            Dictionary<dynamic, dynamic> POIDictionary = new Dictionary<dynamic, dynamic>();

            HttpWebRequest request;
            HttpWebResponse response;

            if (category == "video-interviste")
                return "Categoria " + category + " saltata!";

            try {

                string URL = baseURL + item["geojson"].ToString();
                request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "GET";

                response = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(response.GetResponseStream())) {
                    category_result = streamReader.ReadToEnd();
                    CategoryDictionary = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(category_result);
                }

                if (!String.IsNullOrWhiteSpace(CategoryDictionary["features"].ToString())) {

                    Newtonsoft.Json.Linq.JArray features = CategoryDictionary["features"];
                    URL = baseURL + "RPC2";

                    foreach (Newtonsoft.Json.Linq.JToken poi in features) {

                        request = (HttpWebRequest)WebRequest.Create(URL);
                        request.ContentType = "application/json; charset=utf-8";
                        request.Method = "POST";

                        using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
                            string json = "{\"id\":\"djangorpc\", \"method\" : \"resources.get_poi\", \"params\" : [\"" + poi["properties"]["id"].ToString() + "\"]}";

                            streamWriter.Write(json);
                            streamWriter.Flush();
                        }

                        response = (HttpWebResponse)request.GetResponse();
                        using (var streamReader = new StreamReader(response.GetResponseStream())) {
                            poi_result = streamReader.ReadToEnd();
                            import_result += poi_result + "\n";
                            POIDictionary = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(poi_result);
                        }

                        if (!String.IsNullOrWhiteSpace(POIDictionary["result"].ToString())) {
                            if ((int)POIDictionary["result"]["fields"]["status"] == 1) {

                                //Creo contenuto master (la lingua di default di Orchard dev'essere impostata a "it-IT")
                                //Per la lingua di default non bisogna settare espliticamente la cultura

                                var POIItemIT = _contentManager.New("POI");

                                ((dynamic)POIItemIT).TitlePart.Title = POIDictionary["result"]["fields"]["name_it"].ToString();
                                ((dynamic)POIItemIT).MapPart.LocationInfo = POIDictionary["result"]["fields"]["name_it"].ToString();
                                ((dynamic)POIItemIT).BodyPart.Text = POIDictionary["result"]["fields"]["description_it"].ToString();

                                _contentManager.Create(POIItemIT);

                                TermPart termIT = _contentManager.Get<TermPart>(POITermsDictionary["it"][category]);
                                _taxonomyService.UpdateTerms(POIItemIT, new List<TermPart> { termIT }, "Categoria");

                                //Creo contenuto in francese

                                var POIItemFR = _contentManager.New("POI");

                                ((dynamic)POIItemFR).TitlePart.Title = POIDictionary["result"]["fields"]["name_fr"].ToString();
                                ((dynamic)POIItemFR).MapPart.LocationInfo = POIDictionary["result"]["fields"]["name_fr"].ToString();
                                ((dynamic)POIItemFR).BodyPart.Text = POIDictionary["result"]["fields"]["description_fr"].ToString();

                                _contentManager.Create(POIItemFR);

                                ((LocalizationPart)((dynamic)POIItemFR).LocalizationPart).MasterContentItem = POIItemIT;
                                _localizationService.SetContentCulture(POIItemFR, "fr-FR");

                                TermPart termFR = _contentManager.Get<TermPart>(POITermsDictionary["fr"][category]);
                                _taxonomyService.UpdateTerms(POIItemFR, new List<TermPart> { termFR }, "Categoria");

                                //Creo contenuto in inglese

                                var POIItemEN = _contentManager.New("POI");

                                ((dynamic)POIItemEN).TitlePart.Title = POIDictionary["result"]["fields"]["name_en"].ToString();
                                ((dynamic)POIItemEN).MapPart.LocationInfo = POIDictionary["result"]["fields"]["name_en"].ToString();
                                ((dynamic)POIItemEN).BodyPart.Text = POIDictionary["result"]["fields"]["description_en"].ToString();

                                _contentManager.Create(POIItemEN);

                                ((LocalizationPart)((dynamic)POIItemEN).LocalizationPart).MasterContentItem = POIItemIT;
                                _localizationService.SetContentCulture(POIItemEN, "en-US");

                                TermPart termEN = _contentManager.Get<TermPart>(POITermsDictionary["en"][category]);
                                _taxonomyService.UpdateTerms(POIItemEN, new List<TermPart> { termEN }, "Categoria");

                                //Aggiungo coordinate a mappa

                                string[] coordinates = POIDictionary["result"]["fields"]["geom"].ToString().Replace("POINT (", "").Replace(")", "").Split(null);

                                ((dynamic)POIItemIT).MapPart.Longitude = float.Parse(coordinates[0], CultureInfo.InvariantCulture.NumberFormat);
                                ((dynamic)POIItemIT).MapPart.Latitude = float.Parse(coordinates[1], CultureInfo.InvariantCulture.NumberFormat);

                                ((dynamic)POIItemFR).MapPart.Longitude = float.Parse(coordinates[0], CultureInfo.InvariantCulture.NumberFormat);
                                ((dynamic)POIItemFR).MapPart.Latitude = float.Parse(coordinates[1], CultureInfo.InvariantCulture.NumberFormat);

                                ((dynamic)POIItemEN).MapPart.Longitude = float.Parse(coordinates[0], CultureInfo.InvariantCulture.NumberFormat);
                                ((dynamic)POIItemEN).MapPart.Latitude = float.Parse(coordinates[1], CultureInfo.InvariantCulture.NumberFormat);

                                //Aggiungo immagine

                                string imageUrl = POIDictionary["result"]["fields"]["image_url"].ToString();
                                var webClient = new WebClient();
                                byte[] imageBytes = webClient.DownloadData(imageUrl);
                                var mediaPart = _mediaLibraryService.ImportMedia(new MemoryStream(imageBytes), "POI", imageUrl.Split('/').Last());
                                _contentManager.Create(mediaPart);

                                ((dynamic)POIItemIT).POI.Gallery.Ids = new Int32[] { mediaPart.Id };
                                ((dynamic)POIItemFR).POI.Gallery.Ids = new Int32[] { mediaPart.Id };
                                ((dynamic)POIItemEN).POI.Gallery.Ids = new Int32[] { mediaPart.Id };
                            }
                        }
                    }
                }

                // Disposing items
                CategoryDictionary = null;
                POIDictionary = null;
                request = null;
                response = null;

                return import_result;

            } catch (Exception e) {
                return e.Message;
            }
        }

        private ActionResult ImportTracks() {

            HttpWebRequest request;
            HttpWebResponse response;

            string track_tree_result = "";
            Dictionary<dynamic, dynamic> TrackTreeDictionary = new Dictionary<dynamic, dynamic>();

            TracksTermsDictionary.Add("it",
                                new Dictionary<int, int> {
                                        { 1, 195 },
                                        { 2, 194 },
                                        { 3, 211 },
                                                         });

            TracksTermsDictionary.Add("fr",
                                new Dictionary<int, int> {
                                        { 1, 198 },
                                        { 2, 197 },
                                        { 3, 210 },
                                                         });

            TracksTermsDictionary.Add("en",
                    new Dictionary<int, int> {
                                        { 1, 201 },
                                        { 2, 200 },
                                        { 3, 209 },
                                                         });

            TerrainTypeTermsDictionary.Add("it",
                new Dictionary<string, int> {
                                        { "coated_percent", 214 },
                                        { "track_percent", 216 },
                                        { "offroad_percent", 215 }
                                                         });

            TerrainTypeTermsDictionary.Add("fr",
                new Dictionary<string, int> {
                                        { "coated_percent", 224 },
                                        { "track_percent", 226 },
                                        { "offroad_percent", 225 }
                                                         });

            TerrainTypeTermsDictionary.Add("en",
                new Dictionary<string, int> {
                                        { "coated_percent", 219 },
                                        { "track_percent", 221 },
                                        { "offroad_percent", 220 }
                                                         });

            try {

                string URL = baseURL + "RPC2";
                request = (HttpWebRequest)WebRequest.Create(URL);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";

                using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
                    string json = "{\"id\":\"djangorpc\", \"method\" : \"resources.get_trackcategory_visible_tree\", \"params\" : [\"it\"]}";

                    streamWriter.Write(json);
                    streamWriter.Flush();
                }

                response = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(response.GetResponseStream())) {
                    track_tree_result = streamReader.ReadToEnd();
                    TrackTreeDictionary = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(track_tree_result);
                }

            } catch (Exception e) {
                track_tree_result = e.Message;
            }

            dynamic model = new ExpandoObject();
            model.TreeResult = track_tree_result;
            model.ImportResult = "";

            try {
                if (!String.IsNullOrWhiteSpace(TrackTreeDictionary["result"].ToString())) {

                    Newtonsoft.Json.Linq.JArray category = TrackTreeDictionary["result"];

                    foreach (Newtonsoft.Json.Linq.JToken item in category) {

                        model.ImportResult += importTrack(item) + "\n";

                        foreach (Newtonsoft.Json.Linq.JToken child in item["children"])
                            model.ImportResult += importTrack(child) + "\n";
                    }
                }
            } catch (Exception e) {
                model.ImportResult = e.Message;
            }

            return View("Import", model);
        }

        private string importTrack(Newtonsoft.Json.Linq.JToken item) {

            string track_category_result = "";
            string track_result = "";
            string import_result = "";
            const string baseTrackURL = "http://www.giroparchi.it/it/resource/track/";

            Dictionary<dynamic, dynamic> TrackCategoryDictionary = new Dictionary<dynamic, dynamic>();
            Dictionary<dynamic, dynamic> TrackDictionary = new Dictionary<dynamic, dynamic>();

            HttpWebRequest request;
            HttpWebResponse response;

            try {

                string URL = baseURL + item["geojson"].ToString();

                request = (HttpWebRequest)WebRequest.Create(URL);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "GET";

                response = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(response.GetResponseStream())) {
                    track_category_result = streamReader.ReadToEnd();
                    TrackCategoryDictionary = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(track_category_result);
                }

                if (!String.IsNullOrWhiteSpace(TrackCategoryDictionary["features"].ToString())) {

                    Newtonsoft.Json.Linq.JArray features = TrackCategoryDictionary["features"];
                    URL = baseURL + "RPC2";

                    foreach (Newtonsoft.Json.Linq.JToken poi in features) {

                        request = (HttpWebRequest)WebRequest.Create(URL);
                        request.ContentType = "application/json; charset=utf-8";
                        request.Method = "POST";

                        using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
                            string json = "{\"id\":\"djangorpc\", \"method\" : \"resources.get_track\", \"params\" : [\"" + poi["properties"]["id"].ToString() + "\"]}";

                            streamWriter.Write(json);
                            streamWriter.Flush();
                        }

                        response = (HttpWebResponse)request.GetResponse();
                        using (var streamReader = new StreamReader(response.GetResponseStream())) {
                            track_result = streamReader.ReadToEnd();
                            import_result += track_result + "\n";
                            TrackDictionary = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(track_result);
                        }

                        if (!String.IsNullOrWhiteSpace(TrackDictionary["result"].ToString())) {
                            if ((int)TrackDictionary["result"]["fields"]["status"] == 1) {

                                int difficulty = Convert.ToInt16(TrackDictionary["result"]["fields"]["difficulty_foot"]);

                                //Creo contenuto master (la lingua di default di Orchard dev'essere impostata a "it-IT")
                                //Per la lingua di default non bisogna settare espliticamente la cultura

                                var TrackItemIT = _contentManager.New("Percorso");

                                ((dynamic)TrackItemIT).TitlePart.Title = TrackDictionary["result"]["fields"]["name_it"].ToString();
                                ((dynamic)TrackItemIT).MapPart.LocationInfo = TrackDictionary["result"]["fields"]["name_it"].ToString();

                                string bodyIT = "";
                                if (Convert.ToInt32(TrackDictionary["result"]["fields"]["track_percent"]) > 0) {
                                    bodyIT += _contentManager.Get<TermPart>(TerrainTypeTermsDictionary["it"]["track_percent"]).Name.ToString();
                                    bodyIT += ": " + Convert.ToInt32(TrackDictionary["result"]["fields"]["track_percent"]) + "%<br/>";
                                }
                                if (Convert.ToInt32(TrackDictionary["result"]["fields"]["offroad_percent"]) > 0) {
                                    bodyIT += _contentManager.Get<TermPart>(TerrainTypeTermsDictionary["it"]["offroad_percent"]).Name.ToString();
                                    bodyIT += ": " + Convert.ToInt32(TrackDictionary["result"]["fields"]["offroad_percent"]) + "%<br/>";
                                }
                                if (Convert.ToInt32(TrackDictionary["result"]["fields"]["coated_percent"]) > 0) {
                                    bodyIT += _contentManager.Get<TermPart>(TerrainTypeTermsDictionary["it"]["coated_percent"]).Name.ToString();
                                    bodyIT += ": " + Convert.ToInt32(TrackDictionary["result"]["fields"]["coated_percent"]) + "%<br/>";
                                }
                                if (bodyIT != "")
                                    bodyIT = "Tipologie di terreno:<br/>" + bodyIT;
                                ((dynamic)TrackItemIT).BodyPart.Text = bodyIT;

                                _contentManager.Create(TrackItemIT);

                                ((dynamic)TrackItemIT).Percorso.Descrizionedelpercorso.Value = TrackDictionary["result"]["fields"]["description_it"].ToString();
                                ((dynamic)TrackItemIT).Percorso.PeriodoConsigliato.Value = stripHTML(TrackDictionary["result"]["fields"]["suggested_time_it"].ToString());
                                ((dynamic)TrackItemIT).Percorso.Partenza.Value = TrackDictionary["result"]["fields"]["start_info_it"].ToString();
                                ((dynamic)TrackItemIT).Percorso.Arrivo.Value = TrackDictionary["result"]["fields"]["end_info_it"].ToString();
                                ((dynamic)TrackItemIT).Percorso.Segnavia.Value = stripHTML(TrackDictionary["result"]["fields"]["signals_it"].ToString());

                                TermPart termIT = _contentManager.Get<TermPart>(TracksTermsDictionary["it"][difficulty]);
                                _taxonomyService.UpdateTerms(TrackItemIT, new List<TermPart> { termIT }, "Difficoltadelpercorso");

                                //Creo contenuto in francese

                                var TrackItemFR = _contentManager.New("Percorso");

                                ((dynamic)TrackItemFR).TitlePart.Title = TrackDictionary["result"]["fields"]["name_fr"].ToString();
                                ((dynamic)TrackItemFR).MapPart.LocationInfo = TrackDictionary["result"]["fields"]["name_fr"].ToString();

                                string bodyFR = "";
                                if (Convert.ToInt32(TrackDictionary["result"]["fields"]["track_percent"]) > 0) {
                                    bodyFR += _contentManager.Get<TermPart>(TerrainTypeTermsDictionary["fr"]["track_percent"]).Name.ToString();
                                    bodyFR += ": " + Convert.ToInt32(TrackDictionary["result"]["fields"]["track_percent"]) + "%<br/>";
                                }
                                if (Convert.ToInt32(TrackDictionary["result"]["fields"]["offroad_percent"]) > 0) {
                                    bodyFR += _contentManager.Get<TermPart>(TerrainTypeTermsDictionary["fr"]["offroad_percent"]).Name.ToString();
                                    bodyFR += ": " + Convert.ToInt32(TrackDictionary["result"]["fields"]["offroad_percent"]) + "%<br/>";
                                }
                                if (Convert.ToInt32(TrackDictionary["result"]["fields"]["coated_percent"]) > 0) {
                                    bodyFR += _contentManager.Get<TermPart>(TerrainTypeTermsDictionary["fr"]["coated_percent"]).Name.ToString();
                                    bodyFR += ": " + Convert.ToInt32(TrackDictionary["result"]["fields"]["coated_percent"]) + "%<br/>";
                                }
                                if (bodyFR != "")
                                    bodyFR = "Typologies de terrain:<br/>" + bodyFR;
                                ((dynamic)TrackItemFR).BodyPart.Text = bodyFR;

                                _contentManager.Create(TrackItemFR);

                                ((LocalizationPart)((dynamic)TrackItemFR).LocalizationPart).MasterContentItem = TrackItemIT;
                                _localizationService.SetContentCulture(TrackItemFR, "fr-FR");

                                ((dynamic)TrackItemFR).Percorso.Descrizionedelpercorso.Value = TrackDictionary["result"]["fields"]["description_fr"].ToString();
                                ((dynamic)TrackItemFR).Percorso.PeriodoConsigliato.Value = stripHTML(TrackDictionary["result"]["fields"]["suggested_time_fr"].ToString());
                                ((dynamic)TrackItemFR).Percorso.Partenza.Value = TrackDictionary["result"]["fields"]["start_info_fr"].ToString();
                                ((dynamic)TrackItemFR).Percorso.Arrivo.Value = TrackDictionary["result"]["fields"]["end_info_fr"].ToString();
                                ((dynamic)TrackItemFR).Percorso.Segnavia.Value = stripHTML(TrackDictionary["result"]["fields"]["signals_fr"].ToString());

                                TermPart termFR = _contentManager.Get<TermPart>(TracksTermsDictionary["fr"][difficulty]);
                                _taxonomyService.UpdateTerms(TrackItemFR, new List<TermPart> { termFR }, "Difficoltadelpercorso");

                                //Creo contenuto in inglese

                                var TrackItemEN = _contentManager.New("Percorso");

                                ((dynamic)TrackItemEN).TitlePart.Title = TrackDictionary["result"]["fields"]["name_en"].ToString();
                                ((dynamic)TrackItemEN).MapPart.LocationInfo = TrackDictionary["result"]["fields"]["name_en"].ToString();

                                string bodyEN = "";
                                if (Convert.ToInt32(TrackDictionary["result"]["fields"]["track_percent"]) > 0) {
                                    bodyEN += _contentManager.Get<TermPart>(TerrainTypeTermsDictionary["en"]["track_percent"]).Name.ToString();
                                    bodyEN += ": " + Convert.ToInt32(TrackDictionary["result"]["fields"]["track_percent"]) + "%<br/>";
                                }
                                if (Convert.ToInt32(TrackDictionary["result"]["fields"]["offroad_percent"]) > 0) {
                                    bodyEN += _contentManager.Get<TermPart>(TerrainTypeTermsDictionary["en"]["offroad_percent"]).Name.ToString();
                                    bodyEN += ": " + Convert.ToInt32(TrackDictionary["result"]["fields"]["offroad_percent"]) + "%<br/>";
                                }
                                if (Convert.ToInt32(TrackDictionary["result"]["fields"]["coated_percent"]) > 0) {
                                    bodyEN += _contentManager.Get<TermPart>(TerrainTypeTermsDictionary["en"]["coated_percent"]).Name.ToString();
                                    bodyEN += ": " + Convert.ToInt32(TrackDictionary["result"]["fields"]["coated_percent"]) + "%<br/>";
                                }
                                if (bodyEN != "")
                                    bodyEN = "Terrain types:<br/>" + bodyEN;
                                ((dynamic)TrackItemEN).BodyPart.Text = bodyEN;

                                _contentManager.Create(TrackItemEN);

                                ((LocalizationPart)((dynamic)TrackItemEN).LocalizationPart).MasterContentItem = TrackItemIT;
                                _localizationService.SetContentCulture(TrackItemEN, "en-US");

                                ((dynamic)TrackItemEN).Percorso.Descrizionedelpercorso.Value = TrackDictionary["result"]["fields"]["description_en"].ToString();
                                ((dynamic)TrackItemEN).Percorso.PeriodoConsigliato.Value = stripHTML(TrackDictionary["result"]["fields"]["suggested_time_en"].ToString());
                                ((dynamic)TrackItemEN).Percorso.Partenza.Value = TrackDictionary["result"]["fields"]["start_info_en"].ToString();
                                ((dynamic)TrackItemEN).Percorso.Arrivo.Value = TrackDictionary["result"]["fields"]["end_info_en"].ToString();
                                ((dynamic)TrackItemEN).Percorso.Segnavia.Value = stripHTML(TrackDictionary["result"]["fields"]["signals_en"].ToString());

                                TermPart termEN = _contentManager.Get<TermPart>(TracksTermsDictionary["en"][difficulty]);
                                _taxonomyService.UpdateTerms(TrackItemEN, new List<TermPart> { termEN }, "Difficoltadelpercorso");

                                //Aggiungo campi comuni a tutte le lingue

                                ((dynamic)TrackItemIT).Percorso.DislivelloPositivo.Value = Convert.ToDecimal(TrackDictionary["result"]["fields"]["climb_up"]);
                                ((dynamic)TrackItemIT).Percorso.DislivelloNegativo.Value = Math.Abs(Convert.ToDecimal(TrackDictionary["result"]["fields"]["climb_down"]));
                                ((dynamic)TrackItemIT).Percorso.Lunghezza.Value = Convert.ToInt32(TrackDictionary["result"]["fields"]["length"]);
                                ((dynamic)TrackItemIT).Percorso.QuotaMassima.Value = Convert.ToDecimal(TrackDictionary["result"]["fields"]["max_height"]);

                                ((dynamic)TrackItemFR).Percorso.DislivelloPositivo.Value = Convert.ToDecimal(TrackDictionary["result"]["fields"]["climb_up"]);
                                ((dynamic)TrackItemFR).Percorso.DislivelloNegativo.Value = Math.Abs(Convert.ToDecimal(TrackDictionary["result"]["fields"]["climb_down"]));
                                ((dynamic)TrackItemFR).Percorso.Lunghezza.Value = Convert.ToInt32(TrackDictionary["result"]["fields"]["length"]);
                                ((dynamic)TrackItemFR).Percorso.QuotaMassima.Value = Convert.ToDecimal(TrackDictionary["result"]["fields"]["max_height"]);

                                ((dynamic)TrackItemEN).Percorso.DislivelloPositivo.Value = Convert.ToDecimal(TrackDictionary["result"]["fields"]["climb_up"]);
                                ((dynamic)TrackItemEN).Percorso.DislivelloNegativo.Value = Math.Abs(Convert.ToDecimal(TrackDictionary["result"]["fields"]["climb_down"]));
                                ((dynamic)TrackItemEN).Percorso.Lunghezza.Value = Convert.ToInt32(TrackDictionary["result"]["fields"]["length"]);
                                ((dynamic)TrackItemEN).Percorso.QuotaMassima.Value = Convert.ToDecimal(TrackDictionary["result"]["fields"]["max_height"]);

                                //Aggiungo KML

                                string kmlUrl = baseTrackURL + TrackDictionary["result"]["fields"]["slug"].ToString() + ".kml";
                                var webClient = new WebClient();
                                byte[] kmlBytes = webClient.DownloadData(kmlUrl);
                                MediaPart kmlMediaPart = _mediaLibraryService.ImportMedia(new MemoryStream(kmlBytes), @"Percorsi\KML", TrackDictionary["result"]["fields"]["slug"].ToString() + ".kml", "Document");
                                _contentManager.Create(kmlMediaPart);

                                string kmlString = System.Text.Encoding.Default.GetString(kmlBytes);
                                int coordinatesPosition = kmlString.IndexOf(">", kmlString.IndexOf("<coordinates>")) + 1;
                                int firstPointPositionEnd = kmlString.IndexOf(" ", coordinatesPosition);
                                string[] firstPointCoordinates = kmlString.Substring(coordinatesPosition, firstPointPositionEnd - coordinatesPosition).Split(',');

                                ((dynamic)TrackItemIT).MapPart.Longitude = float.Parse(firstPointCoordinates[0], CultureInfo.InvariantCulture.NumberFormat);
                                ((dynamic)TrackItemIT).MapPart.Latitude = float.Parse(firstPointCoordinates[1], CultureInfo.InvariantCulture.NumberFormat);

                                ((dynamic)TrackItemFR).MapPart.Longitude = float.Parse(firstPointCoordinates[0], CultureInfo.InvariantCulture.NumberFormat);
                                ((dynamic)TrackItemFR).MapPart.Latitude = float.Parse(firstPointCoordinates[1], CultureInfo.InvariantCulture.NumberFormat);

                                ((dynamic)TrackItemEN).MapPart.Longitude = float.Parse(firstPointCoordinates[0], CultureInfo.InvariantCulture.NumberFormat);
                                ((dynamic)TrackItemEN).MapPart.Latitude = float.Parse(firstPointCoordinates[1], CultureInfo.InvariantCulture.NumberFormat);

                                ((dynamic)TrackItemIT).MapPart.MapSourceFile.Ids = new Int32[] { kmlMediaPart.Id };
                                ((dynamic)TrackItemFR).MapPart.MapSourceFile.Ids = new Int32[] { kmlMediaPart.Id };
                                ((dynamic)TrackItemEN).MapPart.MapSourceFile.Ids = new Int32[] { kmlMediaPart.Id };

                                //Aggiungo immagine fittizia

                                ((dynamic)TrackItemIT).Percorso.Gallery.Ids = new Int32[] { 228 };
                                ((dynamic)TrackItemFR).Percorso.Gallery.Ids = new Int32[] { 228 };
                                ((dynamic)TrackItemEN).Percorso.Gallery.Ids = new Int32[] { 228 };
                            }
                        }
                    }
                }

                return import_result;

            } catch (Exception e) {
                return e.Message;
            }
        }

        private string stripHTML(string text) {

            text = Regex.Replace(text, @"<br[^>]+>", " ");
            text = Regex.Replace(text, @"<[^>]+>", "").Trim();
            return text;
        }
    }
}
