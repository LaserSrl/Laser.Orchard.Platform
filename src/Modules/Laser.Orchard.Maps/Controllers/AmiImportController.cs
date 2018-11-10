using Newtonsoft.Json;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.MediaLibrary.Services;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;

namespace Laser.Orchard.Maps.Controllers
{

    [OrchardFeature("Laser.Orchard.Maps.Import")]
    public class AmiImportController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IFieldIndexService _fieldIndexService;
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IRepository<DecimalFieldIndexRecord> _decimalFieldIndexRecord;
        private readonly IRepository<FieldIndexPartRecord> _fieldIndexRecord;
        private readonly ITaxonomyService _taxonomyService;
        private readonly Lazy<IAutorouteService> _autorouteService;

        ILogger Log { get; set; }
        public AmiImportController(IContentManager contentManager, IFieldIndexService fieldIndexService, ITaxonomyService taxonomyService,
                                   IMediaLibraryService mediaLibraryService, IRepository<DecimalFieldIndexRecord> decimalFieldIndexRecord,
                                   IRepository<FieldIndexPartRecord> fieldIndexRecord, Lazy<IAutorouteService> autorouteService)
        {
            _contentManager = contentManager;
            _fieldIndexService = fieldIndexService;
            _mediaLibraryService = mediaLibraryService;
            _decimalFieldIndexRecord = decimalFieldIndexRecord;
            _fieldIndexRecord = fieldIndexRecord;
            _taxonomyService = taxonomyService;
            _autorouteService = autorouteService;

            Log = NullLogger.Instance;
        }

        //
        // GET: /amiimport/pois
        public ActionResult Pois(string jsonFile)
        {
            const string basePath = "~/Modules/Laser.Orchard.Maps/Contents/Ami/";
            Regex contactsPattern = new Regex("(?<address>.+)Tel:(?<phone>.+)Email:(?<email>.+)Web:(?<website>.+)");

            Dictionary<int, int> OutdoorCategoryDictionary = new Dictionary<int, int>();
            Dictionary<int, int> SitiCategoryDictionary = new Dictionary<int, int>();

            SitiCategoryDictionary.Add(1, 17);      // Patrimonio Industriale
            SitiCategoryDictionary.Add(2, 16);      // Arte e storia
            SitiCategoryDictionary.Add(3, 124);     // Parchi e aree naturalistiche
            SitiCategoryDictionary.Add(4, 121);     // Archeologia
            SitiCategoryDictionary.Add(5, 122);     // Beni religiosi
            SitiCategoryDictionary.Add(6, 123);     // Cultura materiale

            OutdoorCategoryDictionary.Add(7, 132);  // Sport montani
            OutdoorCategoryDictionary.Add(8, 130);  // Sport acquatici
            OutdoorCategoryDictionary.Add(9, 128);  // Equitazione
            OutdoorCategoryDictionary.Add(10, 126); // Ciclismo
            OutdoorCategoryDictionary.Add(11, 129); // Escursionismo
            OutdoorCategoryDictionary.Add(12, 125); // Altri sport
            OutdoorCategoryDictionary.Add(13, 127); // Corsa
            OutdoorCategoryDictionary.Add(14, 131); // Sport invernali

            object objPois = DeserializzaDaFileSystemJson(typeof(PoiRootObject), Server.MapPath(basePath + jsonFile));

            PoiRootObject pois = (PoiRootObject)objPois;

            int numContent = 0;
            int newContent = 0;
            int totContents = 0;

            foreach (Point singlePoint in pois.points)
            {
                try
                {
                    totContents++;

                    // Verifico se il POI esiste già. Se esiste lo aggiorno, altrimenti lo creo.
                    var contentId = 0;
                    contentId = _fieldIndexRecord.Fetch(
                        f => f.DecimalFieldIndexRecords.Any<DecimalFieldIndexRecord>(
                            r =>
                                r.PropertyName == "POI.ImportId." &&
                                r.Value == numContent + 1
                        )
                    ).Select(c => c.Id).LastOrDefault();

                    ContentItem content = null;
                    content = _contentManager.Get(contentId);

                    if (content == null)
                    {
                        newContent++;
                        content = _contentManager.New("POI");
                        _contentManager.Create(content, VersionOptions.Latest);
                    }

                    // Inserisco i dati di base
                    ((dynamic)content).TitlePart.Title = singlePoint.title;
                    ((dynamic)content).MapPart.Longitude = (float)singlePoint.longitude;
                    ((dynamic)content).MapPart.Latitude = (float)singlePoint.latitude;
                    ((dynamic)content).MapPart.LocationInfo = singlePoint.title;

                    string description = "";
                    if (!String.IsNullOrWhiteSpace(singlePoint.description))
                        description = description + singlePoint.description.Replace("\n", "<br>");
                    if (!String.IsNullOrWhiteSpace(singlePoint.timePrices))
                    {
                        if (description.Length > 0)
                            description = description + "<br>";
                        description = description + singlePoint.timePrices.Replace("\n", "<br>");
                    }

                    ((dynamic)content).BodyPart.Text = description;
                    ((dynamic)content).POI.Sport.Value = singlePoint.sport;

                    // Genero alias autoroute
                    if (String.IsNullOrWhiteSpace(((dynamic)content).AutoroutePart.DisplayAlias))
                    {
                        ((dynamic)content).AutoroutePart.DisplayAlias = _autorouteService.Value.GenerateAlias(((dynamic)content).AutoroutePart);
                        _autorouteService.Value.PublishAlias(((dynamic)content).AutoroutePart);
                    }

                    // Assegno ImportId e lo indicizzo (necessario per eseguire ricerche su di esso)
                    ((dynamic)content).POI.ImportId.Value = (Decimal)numContent + 1;
                    _fieldIndexService.Set(((dynamic)content).FieldIndexPart, "POI", "ImportId", "", (Decimal)numContent + 1, typeof(Decimal));

                    // Aggiungo le categorie e le foto
                    int termId;
                    List<TermPart> termList = new List<TermPart>();
                    string subFolder = "";

                    foreach (int category in singlePoint.categories)
                    {
                        termId = 0;
                        if (OutdoorCategoryDictionary.TryGetValue(category, out termId))
                            subFolder = "\\Outdoor";
                        else if (SitiCategoryDictionary.TryGetValue(category, out termId))
                            subFolder = "\\Siti";

                        if (termId != 0)
                            termList.Add(_contentManager.Get<TermPart>(termId));
                    }
                    _taxonomyService.UpdateTerms(content, termList, "Categoria");

                    List<Int32> photoIds = new List<Int32>();
                    foreach (var photo in singlePoint.photos)
                    {
                        byte[] imageBytes = System.IO.File.ReadAllBytes(Server.MapPath(basePath + "Foto/" + photo));
                        var mediaPart = _mediaLibraryService.ImportMedia(new MemoryStream(imageBytes), "POI" + subFolder, photo.Replace(" ", ""));
                        _contentManager.Create(mediaPart);
                        photoIds.Add(mediaPart.Id);
                    }
                    ((dynamic)content).POI.Gallery.Ids = photoIds.ToArray();

                    // Aggiungo contatti
                    var contactContentId = 0;
                    contactContentId = _fieldIndexRecord.Fetch(
                                        f => f.DecimalFieldIndexRecords.Any<DecimalFieldIndexRecord>(
                                            r =>
                                                r.PropertyName == "Contatto.ImportId." &&
                                                r.Value == numContent + 1
                                        )
                                    ).Select(c => c.Id).LastOrDefault();

                    ContentItem contacts = null;
                    contacts = _contentManager.Get(contactContentId);

                    if (!String.IsNullOrWhiteSpace(singlePoint.contacts))
                    {
                        Match match = contactsPattern.Match(singlePoint.contacts.Replace("\n", " "));
                        if (match.Success)
                        {
                            if (contacts == null)
                            {
                                contacts = _contentManager.New("Contatto");
                                _contentManager.Create(contacts, VersionOptions.Latest);

                                ((dynamic)contacts).Contatto.ImportId.Value = (Decimal)numContent + 1;
                                _fieldIndexService.Set(((dynamic)contacts).FieldIndexPart, "Contatto", "ImportId", "", (Decimal)numContent + 1, typeof(Decimal));
                            }

                            ((dynamic)contacts).TitlePart.Title = singlePoint.title;
                            ((dynamic)contacts).Contatto.Indirizzo.Value = (match.Groups["address"].Value).Trim();
                            ((dynamic)contacts).Contatto.Telefono.Value = (match.Groups["phone"].Value).Trim();
                            ((dynamic)contacts).Contatto.Email.Value = (match.Groups["email"].Value).Trim();
                            ((dynamic)contacts).Contatto.Note.Value = "";

                            string websiteUrl = (match.Groups["website"].Value).Trim();
                            if (!websiteUrl.StartsWith("http://"))
                                websiteUrl = "http://" + websiteUrl;

                            ((dynamic)contacts).Contatto.SitoWeb.Value = websiteUrl;

                            ((dynamic)content).POI.Contatti.Ids = new Int32[] { contacts.Id };
                            _contentManager.Publish(contacts);
                        }
                        else if (singlePoint.contacts.ToLower().StartsWith("per informazion"))
                        {
                            if (contacts == null)
                            {
                                contacts = _contentManager.New("Contatto");
                                _contentManager.Create(contacts, VersionOptions.Latest);

                                ((dynamic)contacts).Contatto.ImportId.Value = (Decimal)numContent + 1;
                                _fieldIndexService.Set(((dynamic)contacts).FieldIndexPart, "Contatto", "ImportId", "", (Decimal)numContent + 1, typeof(Decimal));
                            }

                            ((dynamic)contacts).TitlePart.Title = singlePoint.title;
                            ((dynamic)contacts).Contatto.Indirizzo.Value = "";
                            ((dynamic)contacts).Contatto.Telefono.Value = "";
                            ((dynamic)contacts).Contatto.Email.Value = "";
                            ((dynamic)contacts).Contatto.Note.Value = singlePoint.contacts;
                            ((dynamic)contacts).Contatto.SitoWeb.Value = "";

                            ((dynamic)content).POI.Contatti.Ids = new Int32[] { contacts.Id };
                            _contentManager.Publish(contacts);
                        }
                        else if (Regex.Match(singlePoint.contacts.Replace("\n", " "), "(?<address>.+)Tel:(?<phone>.+)").Success)
                        {
                            match = Regex.Match(singlePoint.contacts.Replace("\n", " "), "(?<address>.+)Tel:(?<phone>.+)");

                            if (contacts == null)
                            {
                                contacts = _contentManager.New("Contatto");
                                _contentManager.Create(contacts, VersionOptions.Latest);

                                ((dynamic)contacts).Contatto.ImportId.Value = (Decimal)numContent + 1;
                                _fieldIndexService.Set(((dynamic)contacts).FieldIndexPart, "Contatto", "ImportId", "", (Decimal)numContent + 1, typeof(Decimal));
                            }

                            ((dynamic)contacts).TitlePart.Title = singlePoint.title;
                            ((dynamic)contacts).Contatto.Indirizzo.Value = (match.Groups["address"].Value).Trim();
                            ((dynamic)contacts).Contatto.Telefono.Value = (match.Groups["phone"].Value).Trim();
                            ((dynamic)contacts).Contatto.Email.Value = "";
                            ((dynamic)contacts).Contatto.Note.Value = "";
                            ((dynamic)contacts).Contatto.SitoWeb.Value = "";

                            ((dynamic)content).POI.Contatti.Ids = new Int32[] { contacts.Id };
                            _contentManager.Publish(contacts);
                        }
                        else if (Regex.Match(singlePoint.contacts.Replace("\n", " "), "(?<address>.+)Email:(?<email>.+)").Success)
                        {
                            match = Regex.Match(singlePoint.contacts.Replace("\n", " "), "(?<address>.+)Email:(?<email>.+)");

                            if (contacts == null)
                            {
                                contacts = _contentManager.New("Contatto");
                                _contentManager.Create(contacts, VersionOptions.Latest);

                                ((dynamic)contacts).Contatto.ImportId.Value = (Decimal)numContent + 1;
                                _fieldIndexService.Set(((dynamic)contacts).FieldIndexPart, "Contatto", "ImportId", "", (Decimal)numContent + 1, typeof(Decimal));
                            }

                            ((dynamic)contacts).TitlePart.Title = singlePoint.title;
                            ((dynamic)contacts).Contatto.Indirizzo.Value = (match.Groups["address"].Value).Trim();
                            ((dynamic)contacts).Contatto.Telefono.Value = "";
                            ((dynamic)contacts).Contatto.Email.Value = (match.Groups["email"].Value).Trim();
                            ((dynamic)contacts).Contatto.Note.Value = "";
                            ((dynamic)contacts).Contatto.SitoWeb.Value = "";

                            ((dynamic)content).POI.Contatti.Ids = new Int32[] { contacts.Id };
                            _contentManager.Publish(contacts);
                        }
                        else
                        {
                            if (contacts == null)
                            {
                                contacts = _contentManager.New("Contatto");
                                _contentManager.Create(contacts, VersionOptions.Latest);

                                ((dynamic)contacts).Contatto.ImportId.Value = (Decimal)numContent + 1;
                                _fieldIndexService.Set(((dynamic)contacts).FieldIndexPart, "Contatto", "ImportId", "", (Decimal)numContent + 1, typeof(Decimal));
                            }

                            ((dynamic)contacts).TitlePart.Title = singlePoint.title;
                            ((dynamic)contacts).Contatto.Indirizzo.Value = singlePoint.contacts.Trim();
                            ((dynamic)contacts).Contatto.Telefono.Value = "";
                            ((dynamic)contacts).Contatto.Email.Value = "";
                            ((dynamic)contacts).Contatto.Note.Value = "";
                            ((dynamic)contacts).Contatto.SitoWeb.Value = "";

                            ((dynamic)content).POI.Contatti.Ids = new Int32[] { contacts.Id };
                            _contentManager.Publish(contacts);
                        }
                    }
                    else if (contacts != null)
                    {
                        _contentManager.Remove(contacts);
                        ((dynamic)content).POI.Contatti.Ids = new Int32[]{};
                    }

                    _contentManager.Publish(content);
                    numContent++;
                }
                catch (Exception ex2)
                {
                    Log.Error(ex2, "Object Title: " + singlePoint.title);
                }
                finally
                {
                    pois = null;
                }
            }

            pois = null;

            var model = new ImportResult { TotContents = totContents, NewContents = newContent, ElaboratedContents = numContent };
            return View(model);
        }

        private object DeserializzaDaFileSystemJson(Type tipoPrincipale, string filename)
        {
            try
            {
                using (StreamReader file = System.IO.File.OpenText(filename))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return serializer.Deserialize(file, tipoPrincipale);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public class Point
        {
            public string description { get; set; }
            public List<int> categories { get; set; }
            public string timePrices { get; set; }
            public List<string> photos { get; set; }
            public double longitude { get; set; }
            public double latitude { get; set; }
            public string contacts { get; set; }
            public string title { get; set; }
            public string sport { get; set; }
        }

        public class Category
        {
            public string name { get; set; }
            public string imageName { get; set; }
            public int identifier { get; set; }
            public bool outdoor { get; set; }
        }

        public class PoiRootObject
        {
            public List<Point> points { get; set; }
            public List<Category> categories { get; set; }
        }

        public class ImportResult
        {
            public int ElaboratedContents { get; set; }
            public int NewContents { get; set; }
            public int TotContents { get; set; }
        }
    }
}