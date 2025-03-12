using Laser.Orchard.Mobile.Models;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Laser.Orchard.Mobile.Controllers {
    public class category {
        public string testo { get; set; }
        public string valore { get; set; }
        public bool flag { get; set; }
    }

    [WebApiKeyFilter(false)]
    public class TaxonomiePushController : ApiController {

        public Localizer T { get; set; }
        private readonly IOrchardServices _orchardServices;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IUtilsServices _utilsServices;

        public TaxonomiePushController(
            IOrchardServices orchardServices,
            ITaxonomyService taxonomyService,
            IAuthenticationService authenticationService,
            ICsrfTokenHelper csrfTokenHelper,
            IUtilsServices utilsServices) {
            _orchardServices = orchardServices;
            _taxonomyService = taxonomyService;
            _authenticationService = authenticationService;
            T = NullLocalizer.Instance;
            _csrfTokenHelper = csrfTokenHelper;
            _utilsServices = utilsServices;
        }



        /// <summary>
        /// Metodo per generare l'elenco di checkbox delle tassonomie attualmente selezionate
        /// http://localhost/Orchard.Community/expoincitta/api/laser.orchard.mobile/taxonomiepush/?Language=en-US
        /// </summary>
        /// <param name="Language"></param>
        /// <returns></returns>

        //public List<category> Get(string Language) {
        public dynamic Get(string Language) {
            Language = Language ?? "";
            var authenticatedUser = _authenticationService.GetAuthenticatedUser();
            if (authenticatedUser != null) {
                #region [get taxonomy linked to user part of type declared on setting of pushmobile]
                string taxonomyId = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().TaxonomyName;
                string taxonomyname = _taxonomyService.GetTaxonomy(Convert.ToInt32(taxonomyId)).Name;
                string nomefield = authenticatedUser.ContentItem.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(TaxonomyField).Name && f.PartFieldDefinition.Settings["TaxonomyFieldSettings.Taxonomy"] == taxonomyname)).Cast<TaxonomyField>().FirstOrDefault().Name;
                #endregion
                List<category> elements = new List<category>();

                List<TermPart> cata = _taxonomyService.GetTermsForContentItem(authenticatedUser.Id, nomefield).ToList();
                //  List<TermPart> cata = ((dynamic)((dynamic)(authenticatedUser.ContentItem)).User).Pushcategories.Terms;
                List<string> ListCategory = cata.Select(x => (string)((dynamic)x).tipoUnivoco.Value).ToList();
                var taxobase = GetPushTaxonomy(Language);
                foreach (var term in taxobase.Terms) {
                    elements.Add(new category() { testo = term.Name, valore = ((dynamic)term).tipoUnivoco.Value, flag = ListCategory.Contains(((dynamic)term).tipoUnivoco.Value) });
                }
                return elements;
            }
            else {
                // throw new OrchardSecurityException(T("Can't retrieve user category, user not logged in"));
                return (_utilsServices.GetResponse(ResponseType.InvalidUser));

            }
        }

        /// <summary>
        /// esempio di json da passare
        /// [{"testo":"cat1 push eng","valore":"Categorie Push - cat1","flag":true},{"testo":"cat2 push eng","valore":"Categorie Push - cat2","flag":false}]
        /// </summary>
        /// <param name="elencocategorie"></param>
        /// <returns></returns>
        public Response Post([FromBody] List<category> elencocategorie) {
            //  try {
            if (_csrfTokenHelper.DoesCsrfTokenMatchAuthToken()) {
                var authenticatedUser = _authenticationService.GetAuthenticatedUser();
                if (authenticatedUser != null) {
                    List<string> ElencoCategorie = elencocategorie.Where(y => y.flag).Select(x => x.valore).ToList();
                    string taxonomyId = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().TaxonomyName;
                    var taxobase = _taxonomyService.GetTaxonomy(Convert.ToInt32(taxonomyId)); // _taxonomyService.GetTaxonomyByName("Categorie Push");
                    IEnumerable<TermPart> otp = _taxonomyService.GetTerms(taxobase.Id);
                    List<TermPart> ltp = new List<TermPart>();
                    foreach (TermPart tp in otp) {
                        if (ElencoCategorie.Contains(((dynamic)tp).tipoUnivoco.Value))
                            ltp.Add(tp);
                    }
                    #region [get taxonomy linked to user part of type declared on setting of pushmobile]
                    //   string taxonomyId = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().TaxonomyName;
                    string taxonomyname = _taxonomyService.GetTaxonomy(Convert.ToInt32(taxonomyId)).Name;
                    string nomefield = authenticatedUser.ContentItem.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(TaxonomyField).Name && f.PartFieldDefinition.Settings["TaxonomyFieldSettings.Taxonomy"] == taxonomyname)).Cast<TaxonomyField>().FirstOrDefault().Name;
                    #endregion
                    _taxonomyService.UpdateTerms(authenticatedUser.ContentItem, ltp, nomefield);
                    return (_utilsServices.GetResponse(ResponseType.Success));
                }
                else
                    return (_utilsServices.GetResponse(ResponseType.InvalidUser));

            }
            else
                return (_utilsServices.GetResponse(ResponseType.InvalidXSRF));
            //}
            //catch (Exception ex) {
            //    return (_utilsServices.GetResponse { Success = false, Message = "Error: " + ex.Message });
            //}
        }

        #region [internal function]

        /// <summary>
        /// Recupera la tassonomia valida in lingua
        /// </summary>
        private TaxonomyPart GetPushTaxonomy(string Language) {
            var taxobase = _taxonomyService.GetTaxonomy(Convert.ToInt32(_orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().TaxonomyName));
            if (Language != "") {
                int idmaster = taxobase.Id;
                if (((dynamic)taxobase.ContentItem).LocalizationPart.MasterContentItem != null) {
                    idmaster = ((dynamic)taxobase.ContentItem).LocalizationPart.MasterContentItem.Id;
                }
                if (((dynamic)taxobase.ContentItem).LocalizationPart.Culture.Culture != Language) {
                    taxobase = _taxonomyService.GetTaxonomies().Where(x => (x.Id == idmaster || (((dynamic)x.ContentItem).LocalizationPart.MasterContentItem != null && ((dynamic)x.ContentItem).LocalizationPart.MasterContentItem.Id == idmaster)) && ((dynamic)x.ContentItem).LocalizationPart.Culture.Culture == Language).FirstOrDefault();
                }
            }
            return taxobase;
        }
        #endregion
    }
}