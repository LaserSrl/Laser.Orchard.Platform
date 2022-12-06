using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Projections.Models;
using System.Dynamic;
using Orchard.Fields.Fields;
using Orchard.ContentPicker.Fields;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Localization.Models;
using Orchard.MediaLibrary.Fields;
using Orchard.Taxonomies.Services;
using Orchard.Logging;
using Orchard.Fields.Settings;
using Laser.Orchard.StartupConfig.ViewModels;

namespace Laser.Orchard.StartupConfig.Services {
    public class ContentExtensionsServices : IContentExtensionsServices {
        private string[] allowedFiles = new string[] { "jpg", "png", "gif", "doc", "docx", "xls", "xlsx", "pdf", "mov", "mp4", "mpg", "mpeg", "avi", "3gp", "mp4v", "m4v", "m4a", "aac", "jpeg", "bmp", "wmv", "wav", "mp3" };
        private string[] ProtectedPart = new string[] { "commonpart", "autoroutepart", "userrolespart" };
        private readonly IContentManager _contentManager;
        private readonly IRepository<StringFieldIndexRecord> _stringFieldIndexRepository;
        private readonly IRepository<FieldIndexPartRecord> _fieldIndexRepository;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IUtilsServices _utilsServices;



        public ILogger Log { get; set; }

        public ContentExtensionsServices(IContentManager contentManager
            , IRepository<StringFieldIndexRecord> stringFieldIndexRepository,
            IRepository<FieldIndexPartRecord> fieldIndexRepository,
            ITaxonomyService taxonomyService,
            IUtilsServices utilsServices) {
            _contentManager = contentManager;
            _fieldIndexRepository = fieldIndexRepository;
            _stringFieldIndexRepository = stringFieldIndexRepository;
            _taxonomyService = taxonomyService;
            Log = NullLogger.Instance;
            _utilsServices = utilsServices;
        }
        public IEnumerable<ParentContent> ContentPickerParents(int contentId, string[] contentTypes) {

            var normalizedContentTypes = String.Join(",", contentTypes.Select(s => !s.EndsWith(".") ? "'" + s.Trim() + ".'" : "'" + s.Trim() + "'"));
            var parents = _fieldIndexRepository.Table
                .Where(w => w.StringFieldIndexRecords.Any(c => normalizedContentTypes.Contains(c.PropertyName) && c.Value.Contains("{" + contentId + "}")))
                .ToList()
                .Select(s =>
                    new ParentContent {
                        FieldNames = s.StringFieldIndexRecords.Where(w2 => normalizedContentTypes.Contains(w2.PropertyName) && w2.Value != null && w2.Value.Contains("{" + contentId + "}")).Select(names => names.PropertyName).ToArray(),
                        ParentContentItem = _contentManager.Get<ContentItem>(s.Id, VersionOptions.Published)
                    }).ToArray();

            /// If a content has been unpublished the ParentContentItem in the list is null, 
            /// that results in an exception when serializing, so the null values must be filtered out                        
            return parents.Where(p => p.ParentContentItem != null);
        }

        public void StoreInspectExpandoFields(List<ContentPart> listpart, string key, object value, ContentItem theContentItem) {
            var fields = listpart.SelectMany(x => x.Fields.Where(f => f.Name == key));
                    if (fields != null) {
                        var fieldObj = fields.FirstOrDefault();
                        if (fieldObj != null) {
                            // provo a registrare il dato in uno dei fields
                            // non posso fare questo
                            //      fieldObj.GetType().GetProperty("Value").SetValue(fieldObj, value, null);
                            // perchè non regge il tipo nullabile
                            string tipofield = fieldObj.GetType().Name;
                            if (tipofield == typeof(EnumerationField).Name) {
                                RegistraValoreEnumerator(fieldObj, "SelectedValues", value);
                            }
                            else
                                if (tipofield == typeof(DateTimeField).Name) {
                                    RegistraValore(fieldObj, "DateTime", value);
                                }
                                else {
                                    if (tipofield == typeof(ContentPickerField).Name || tipofield == typeof(MediaLibraryPickerField).Name) {
                                        RegistraValoreContentPickerField(fieldObj, "Ids", value);
                                    }
                                    else {
                                        if (tipofield == typeof(TaxonomyField).Name) {
                                            var taxobase = _taxonomyService.GetTaxonomyByName(fieldObj.PartFieldDefinition.Settings["TaxonomyFieldSettings.Taxonomy"]);
                                            List<TaxoVM> second = ConvertToVM((List<dynamic>)value);
                                            List<Int32> ElencoCategorie = second.Where(y => y.flag).Select(x => x.Id).ToList();
                                            List<TermPart> ListTermPartToAdd = new List<TermPart>();
                                            if (ElencoCategorie.Count > 0) {
                                                var taxo_sended_user = _taxonomyService.GetTaxonomy(_taxonomyService.GetTerm(ElencoCategorie.FirstOrDefault()).TaxonomyId);

                                                foreach (Int32 idtermine in ElencoCategorie) {
                                                    TermPart termine_selezionato = taxo_sended_user.Terms.Where(x => x.Id == idtermine).FirstOrDefault();
                                                    #region [ Tassonomia in Lingua ]

                                                    if (theContentItem.As<LocalizationPart>() == null || theContentItem.ContentType == "User") { // se il contenuto non ha localization oppure è user salvo il mastercontent del termine
                                                        Int32 idmaster = 0;
                                                        if (termine_selezionato.ContentItem.As<LocalizationPart>() == null) {
                                                            idmaster = termine_selezionato.ContentItem.Id;
                                                        }
                                                        else if (termine_selezionato.ContentItem.As<LocalizationPart>().MasterContentItem == null)
                                                            idmaster = termine_selezionato.ContentItem.As<LocalizationPart>().Id;
                                                        else
                                                            idmaster = termine_selezionato.ContentItem.As<LocalizationPart>().MasterContentItem.Id;
                                                        TermPart toAdd = taxobase.Terms.Where(x => x.Id == idmaster).FirstOrDefault();
                                                        if (toAdd == null)
                                                            toAdd = taxobase.Terms.Where(x => x.ContentItem.As<LocalizationPart>().MasterContentItem.Id == idmaster).FirstOrDefault();
                                                        ListTermPartToAdd.Add(toAdd);
                                                    }
                                                    else { // se il contenuto ha localization e non è user salvo il termine come mi viene passato 
                                                        // TODO: testare pertinenza della lingua Contenuto in italianao=>termine in italiano
                                                        TermPart toAdd = termine_selezionato;
                                                        ListTermPartToAdd.Add(toAdd);
                                                    }
                                                    #endregion
                                                }
                                            }
                                            _taxonomyService.UpdateTerms(theContentItem, ListTermPartToAdd, fieldObj.Name);
                                        }
                                        else {
                                            RegistraValore(fieldObj, "Value", value);
                                        }
                                    }
                                }
                        }
                        else {
                            //provo a registrare il dato nella proprieta dello current user
                            //RegistraValore(currentUser, key, value);
                            if (!(key.IndexOf(".") > 0) && key.ToLower() != "contenttype" && key.ToLower() != "language") {
                                throw new Exception("Field " + key + " not in contentitem");
                            }
                            //   if (TheContentItem.ContentType == "User")
                            //      
                        }
                    }
                    else {
                        //provo a registrare il dato nella proprieta dello current user
                        // RegistraValore(currentUser, key, value);
                        // currentUser.GetType().GetProperty(key).SetValue(currentUser, value, null);
                        if (!(key.IndexOf(".") > 0) && key.ToLower() != "contenttype" && key.ToLower() != "language") {
                            throw new Exception("Field " + key + " not in contentitem");
                        }
                        //    if (TheContentItem.ContentType == "User")
                        //       throw new Exception("Field " + key + " not in user contentitem");
                    }
        }

        public Response StoreInspectExpando(ExpandoObject theExpando, ContentItem theContentItem) {
            try {
                foreach (var kvp in theExpando) {
                    string key = kvp.Key.ToString();
                    string valueType = kvp.Value.GetType().Name;
                    object value = kvp.Value;
                    if (kvp.Value is ExpandoObject) {
                        StoreInspectExpando(kvp.Value as ExpandoObject, theContentItem);
                    }

                    // provo a registrare nel profile part altrimenti provo a registrare nel IUser
                    //currentUser.GetType().GetProperty(key).SetValue(currentUser,value, null);

                    StoreInspectExpandoFields(theContentItem.Parts.ToList(), key, value, theContentItem);
                
                }
            }
            catch (Exception ex) {
                Log.Error("Startupconfig -> ContentExtensionService -> StoreInspectExpando : " + ex.Message + " <Stack> " + ex.StackTrace);
                return (_utilsServices.GetResponse(ResponseType.None,"Error:" + ex.Message));
            }
            //    if (TheContentItem.ContentType != "User")
            return StoreInspectExpandoPart(theExpando, theContentItem);
            //  return (_utilsServices.GetResponse(ResponseType.Success));
        }


        private Response StoreInspectExpandoPart(ExpandoObject theExpando, ContentItem TheContentItem) {
            try {
                foreach (var kvp in theExpando) {
                    string key = kvp.Key.ToString();
                    if (key.IndexOf(".") > 0) {
                        string valueType = kvp.Value.GetType().Name;
                        object value = kvp.Value;
                        //if (kvp.Value is ExpandoObject) {
                        //    StoreInspectExpandoPart(kvp.Value as ExpandoObject, TheContentItem);
                        //}

                        //if (key.IndexOf(".") > 0) {

                            StoreLikeDynamic(key, value, TheContentItem);

                        //}
                    }
                }
            }
            catch (Exception ex) {
                return _utilsServices.GetResponse(ResponseType.None,ex.Message); 
            }
            return (_utilsServices.GetResponse(ResponseType.Success));
        }
        private void StoreLikeDynamic(string key, object value, ContentItem TheContentItem) {
            string[] ListProperty = key.Split('.');
            if (!ProtectedPart.Contains(ListProperty[0].ToLower())) {
                dynamic subobject = TheContentItem.Parts.Where(x => x.PartDefinition.Name == ListProperty[0]).FirstOrDefault();
                Int32 numparole = ListProperty.Count();
                for (int i = 1; i < numparole; i++) {
                    string property = ListProperty[i];
                    if (i != numparole - 1)
                        subobject = subobject.GetType().GetProperty(property);
                    else {
                        try { // é una proprietà 
                            subobject.GetType().GetProperty(property).SetValue(subobject, Convert.ChangeType(value, subobject.GetType().GetProperty(property).PropertyType), null);
                            // potrei ancora tentare di scrivere direttamente con
                            // subobject.GetType().GetProperty(property).SetValue(subobject, value, null);
                        }
                        catch { // é un field della parte
                                List<ContentPart> lcp = new List<ContentPart>();
                                lcp.Add((ContentPart)subobject);
                                StoreInspectExpandoFields(lcp, property, value, TheContentItem);
                          }
                    }
                }
            }
        }
        //private void RegistraValoreArray(object obj, string key, object[] value) {
        //    var property = obj.GetType().GetProperty(key);
        //    if (property != null) {
        //        Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        //        for (Int32 i = 0; i <= value.Length; i++) {
        //            object safeValue = (value[i] == null) ? null : Convert.ChangeType(value[i], t);
        //            property.SetValue(obj, safeValue, null);
        //        }
        //    }
        //}


        private void RegistraValoreEnumerator(object obj, string key, object value) {
            ListMode listmode = ((dynamic)obj).PartFieldDefinition.Settings.GetModel<EnumerationFieldSettings>().ListMode;
            if (listmode != ListMode.Listbox && listmode != ListMode.Checkbox) {
                key = "Value";
            }
            var property = obj.GetType().GetProperty(key);
            if (property != null) {
                Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
             //   ListMode listmode = ((dynamic)obj).PartFieldDefinition.Settings.GetModel<EnumerationFieldSettings>().ListMode;
                if (listmode != ListMode.Listbox && listmode != ListMode.Checkbox) {
                    try {
                        if (((dynamic)value).Count == 1) {
                            value = ((dynamic)value)[0];
                        }
                    }
                    catch { }
                   // string myvalue = ((List<object>)value).Select(x => x.ToString()).FirstOrDefault();
                  //  object safeValue = (value == null) ? null : Convert.ChangeType(myvalue, t);
                  //  property.SetValue(obj, myvalue, null);
                    RegistraValore(obj, key, value);
                }
                else {
                    if (t.Name == "String[]") { // caso di enumerationfield con multivalue
                        object safeValue = (value == null) ? null : ((List<object>)value).Select(x => x.ToString()).ToArray();
                        property.SetValue(obj, safeValue, null);
                    }

                }

            }


        }

        private void RegistraValore(object obj, string key, object value) {
            // non posso fare questo
            //      fieldObj.GetType().GetProperty("Value").SetValue(fieldObj, value, null);
            // perchè non regge il tipo nullabile
            var property = obj.GetType().GetProperty(key);
            if (property != null) {
                Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                object safeValue = (value == null) ? null : Convert.ChangeType(value, t);
                property.SetValue(obj, safeValue, null);
            }
        }



        private void RegistraValoreContentPickerField(object obj, string key, object value) {
            // Convert ha varie implementazioni e non sa castare il tipo correttamente
            var property = obj.GetType().GetProperty(key);
            if (property != null) {
                Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                object safeValue = (value == null) ? null : Convert.ChangeType(((List<object>)value).Select(x => Convert.ToInt32(x)).ToArray(), t);
                property.SetValue(obj, safeValue, null);
            }
        }
        //private TaxoVM FindTaxoVM(List<TaxoVM> elements, Int32 idToFind) {
        //    if (elements != null) {
        //        foreach (TaxoVM myterm in elements) {
        //            if (myterm.Id == idToFind)
        //                return myterm;
        //            else
        //                return FindTaxoVM(myterm.child, idToFind);
        //        }
        //        return null;
        //    }
        //    else
        //        return null;
        //}
        private List<TaxoVM> ConvertToVM(List<dynamic> obj) {
            List<TaxoVM> listatvm = new List<TaxoVM>();
            foreach (dynamic el in obj) {
                TaxoVM newel = new TaxoVM();
                try {
                    newel.flag = el.flag;
                }
                catch {
                }
                newel.Id = Convert.ToInt32(el.Id);
                try {
                    newel.testo = el.testo ?? "";
                }
                catch {
                }
                try {
                    newel.valore = (string)el.valore ?? "";
                }
                catch {
                }
                //  newel.child = ConvertToVM((List < dynamic >)  el.child);
                listatvm.Add(newel);
                try {
                    if (el.child != null) {
                        foreach (TaxoVM tv in ConvertToVM((List<dynamic>)el.child)) {
                            listatvm.Add(tv);
                        }
                    }
                }
                catch {
                }
            }
            return listatvm;
        }
        public bool FileAllowed(string filename) {
            if (filename != null && filename.IndexOf('.') > 0)
                return allowedFiles.Contains(filename.ToLower().Split('.').Last());
            else
                return false;
        }
    }
}