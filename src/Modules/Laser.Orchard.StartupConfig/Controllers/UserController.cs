using Laser.Orchard.StartupConfig.Handlers;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentPicker.Fields;
using Orchard.Fields.Fields;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.MediaLibrary.Fields;
using Orchard.Security;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Users.Events;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace Laser.Orchard.StartupConfig.Controllers {
    [WebApiKeyFilter(false)]
    public class UserController : ApiController {
        private readonly IAuthenticationService _authenticationService;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IOrchardServices _orchardServices;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IContentExtensionsServices _contentExtensionsServices;
        private readonly IUtilsServices _utilsServices;
        private readonly IContactRelatedEventHandler _contactEventHandler;
        private readonly ILocalizedStringManager _localizedStringManager;

        public Localizer T { get; set; }
        public ILogger Log { get; set; }

        public UserController(
            ICsrfTokenHelper csrfTokenHelper,
            IOrchardServices orchardServices,
            ITaxonomyService taxonomyService,
            ILocalizedStringManager localizedStringManager,
            IAuthenticationService authenticationService,
            IContentExtensionsServices contentExtensionsServices,
            IUtilsServices utilsServices,
            IContactRelatedEventHandler contactEventHandler) {
            _csrfTokenHelper = csrfTokenHelper;
            _orchardServices = orchardServices;
            _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
            _localizedStringManager = localizedStringManager;
            Log = NullLogger.Instance;
            _authenticationService = authenticationService;
            _contentExtensionsServices = contentExtensionsServices;
            _utilsServices = utilsServices;
            _contactEventHandler = contactEventHandler;
        }

        public dynamic Get(string Language = "it-IT") {
            var eObj = new ExpandoObject() as IDictionary<string, Object>;
            var currentUser = _authenticationService.GetAuthenticatedUser();
            //  var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser == null)
                return (_utilsServices.GetResponse(ResponseType.InvalidUser));// { Message = "Error: No current User", Success = false,ErrorCode=ErrorCode.InvalidUser,ResolutionAction=ResolutionAction.Login });

            #region Tutti i field

            var fields = currentUser.ContentItem.Parts.SelectMany(x => x.Fields);
            string tipofield = "";
            foreach (ContentField singleField in fields) {
                tipofield = singleField.GetType().Name;
                if (tipofield == typeof(DateTimeField).Name)
                    eObj.Add(singleField.Name, ((dynamic)singleField).DateTime);
                else
                    if (tipofield == typeof(ContentPickerField).Name || tipofield == typeof(MediaLibraryPickerField).Name)
                        eObj.Add(singleField.Name, ((dynamic)singleField).Ids);
                    else
                        if (tipofield == typeof(TaxonomyField).Name) {

                            #region Tassonomia in Lingua

                            var taxobase = _taxonomyService.GetTaxonomyByName(singleField.PartFieldDefinition.Settings["TaxonomyFieldSettings.Taxonomy"]);
                            // seleziono la tassonomia in lingua
                            int idmaster = taxobase.Id;
                            if (((dynamic)taxobase.ContentItem).LocalizationPart.MasterContentItem != null) {
                                idmaster = ((dynamic)taxobase.ContentItem).LocalizationPart.MasterContentItem.Id;
                            }
                            if (((dynamic)taxobase.ContentItem).LocalizationPart.Culture.Culture != Language) {
                                taxobase = _taxonomyService.GetTaxonomies().Where(x => (x.Id == idmaster || (((dynamic)x.ContentItem).LocalizationPart.MasterContentItem != null && ((dynamic)x.ContentItem).LocalizationPart.MasterContentItem.Id == idmaster)) && ((dynamic)x.ContentItem).LocalizationPart.Culture.Culture == Language).FirstOrDefault();
                            }
                            List<TermPart> cata = _taxonomyService.GetTermsForContentItem(currentUser.Id, singleField.Name).ToList();

                            List<string> ListCategory = new List<string>();
                            ListCategory = cata.Select(x => x.Id.ToString()).ToList();
                            List<TaxoVM> elements = new List<TaxoVM>();
                            foreach (var term in taxobase.Terms) {
                                if (term.FullPath == "/" + term.Id)
                                    elements.Add(new TaxoVM() { Id = term.Id, testo = term.Name, valore = term.Id.ToString(), flag = ListCategory.Contains(term.Id.ToString()) });
                                else {
                                    Int32 idtermfather = Convert.ToInt32(term.FullPath.Split('/')[term.FullPath.Split('/').Length - 2]);
                                    FindTaxoVM(elements, idtermfather).child.Add(new TaxoVM() { Id = term.Id, testo = term.Name, valore = term.Id.ToString(), flag = ListCategory.Contains(term.Id.ToString()) });
                                }
                            }
                            eObj.Add(singleField.Name, elements);

                            #endregion Tassonomia in Lingua
                        }
                        else
                            if (tipofield == typeof(EnumerationField).Name) {
                                eObj.Add(singleField.Name, ((dynamic)singleField).Value);
                                string[] elencovalori = singleField.PartFieldDefinition.Settings["EnumerationFieldSettings.Options"].Split(new string[] { "\r\n" }, StringSplitOptions.None);
                                //      Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                                List<string> elencoValoriInLingua = new List<string>();
                                foreach (string val in elencovalori) {
                                    elencoValoriInLingua.Add(_localizedStringManager
                                        .GetLocalizedString(new string[] { "UserEnumeratore" }, val, Language)
                                        .Format);    //T.Target.(elencovalori[0],)
                                }
                                eObj.Add(singleField.Name + "valori", singleField.PartFieldDefinition.Settings["EnumerationFieldSettings.Options"]);
                                eObj.Add(singleField.Name + "valoriLang", String.Join("\r\n", elencoValoriInLingua));
                            }
                            else
                                eObj.Add(singleField.Name, ((dynamic)singleField).Value);
            }

            #endregion Tutti i field

            return eObj;
        }

        private TaxoVM FindTaxoVM(List<TaxoVM> elements, Int32 idToFind) {
            if (elements != null) {
                foreach (TaxoVM myterm in elements) {
                    if (myterm.Id == idToFind)
                        return myterm;
                    else
                        return FindTaxoVM(myterm.child, idToFind);
                }
                return null;
            }
            else
                return null;
        }

        /// <summary>
        /// test in feedler
        /// User-Agent: Fiddler
        ///Content-Type: application/json
        ///Host: localhost
        ///Content-Length: 105
        ///Cookie: .ASPXAUTH=3BDDACF3339764AE73D8AF6A9992CE0C34247D3107C7E66561BCF0A8ACB81C0B0708A274B5B5A57B3C9425AA98825AF5429AE0DE9958CEC8923B998667D89184755E3446DAA5832C7C16C519ABDD1981E919AC22E1A81D277F615F3240264D5FB0B46F174EEC84EB839387462EA250CC71B856178CE26EF0EDEFD6B00E40FACF
        ///X-XSRF-TOKEN: Tnaq+qFFu+B/NPrfJZnTg5FoATHDCrTP3aAAXA90MiS1vpkk5y2QwfRJ5aAmqu4n7GFv+6CcUos+klKlOXBu1A==
        ///{"cpf":[424,414],"Referente":"anonio","Privacy":true,"testInput":"aaaasasas","TestNumero":633}
        /// </summary>
        /// <param name="eObj"></param>
        /// <returns></returns>
        public Response Post(ExpandoObject eObj) {
            if (_csrfTokenHelper.DoesCsrfTokenMatchAuthToken()) {
                var currentUser = _orchardServices.WorkContext.CurrentUser;
                if (currentUser == null)
                    return _utilsServices.GetResponse(ResponseType.InvalidUser);
                var result = _contentExtensionsServices.StoreInspectExpando(eObj, currentUser.ContentItem);

                // solleva l'evento di sincronizzazione del contact dopo l'aggiornamento del numero di telefono
                _contactEventHandler.Synchronize(currentUser);

                return result;
            }
            else {
                return (_utilsServices.GetResponse(ResponseType.InvalidXSRF));// { Message = "Invalid Token/csrfToken", Success = false, ErrorCode=ErrorCode.InvalidXSRF,ResolutionAction=ResolutionAction.Login });
            }
        }
    }
}