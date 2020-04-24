using Laser.Orchard.ContentExtension.Services;
using Laser.Orchard.StartupConfig.RazorBase.Services;
using Laser.Orchard.StartupConfig.RazorCodeExecution.Services;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Laser.Orchard.UsersExtensions.Filters;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Contents.Settings;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Fields.Fields;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.MediaLibrary.Fields;
using Orchard.Security;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Http;
using OrchardCore = Orchard.Core;
using CorePermissions = Orchard.Core.Contents.Permissions;
using Orchard.ContentManagement.Aspects;

namespace Laser.Orchard.ContentExtension.Controllers {

    [WebApiKeyFilter(true)]
    public class ContentItemController : ApiController {
        private readonly IAuthenticationService _authenticationService;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IOrchardServices _orchardServices;
        private readonly Lazy<IAutorouteService> _autorouteService;
        private readonly ILocalizationService _localizationService;
        private readonly ICultureManager _cultureManager;
        private readonly ShellSettings _shellSettings;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IContentExtensionService _contentExtensionService;
        private readonly ILocalizedStringManager _localizedStringManager;
        private readonly IUtilsServices _utilsServices;
        private readonly ITransactionManager _transactionManager;
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;
        private readonly IRazorTemplateManager _razorTemplateManager;
        private readonly INotifier _notifier;
        private readonly IRazorBaseService _razorService;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizer _authorizer;

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ContentItemController(
            ShellSettings shellSettings,
            INotifier notifier,
            ICsrfTokenHelper csrfTokenHelper,
            IOrchardServices orchardServices,
            IAuthenticationService authenticationService,
            IContentExtensionService contentExtensionService,
            Lazy<IAutorouteService> autorouteService,
            ILocalizationService localizationService,
            ICultureManager cultureManager,
            IUtilsServices utilsServices,
            IContentDefinitionManager contentDefinitionManager,
            ITaxonomyService taxonomyService,
            ILocalizedStringManager localizedStringManager,
            ITransactionManager transactionManager,
            Lazy<IEnumerable<IContentHandler>> handlers,
            IRazorTemplateManager razorTemplateManager,
            IRazorBaseService razorService,
            IContentManager contentManager,
            IAuthorizer authorizer
            ) {

            _razorTemplateManager = razorTemplateManager;
            _localizedStringManager = localizedStringManager;
            _taxonomyService = taxonomyService;
            _contentDefinitionManager = contentDefinitionManager;
            _shellSettings = shellSettings;
            _csrfTokenHelper = csrfTokenHelper;
            _orchardServices = orchardServices;
            _authenticationService = authenticationService;
            _contentExtensionService = contentExtensionService;
            _autorouteService = autorouteService;
            _localizationService = localizationService;
            _cultureManager = cultureManager;
            _utilsServices = utilsServices;
            _transactionManager = transactionManager;
            _handlers = handlers;
            _notifier = notifier;
            _razorService = razorService;
            _contentManager = contentManager;
            _authorizer = authorizer;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IEnumerable<IContentHandler> Handlers {
            get { return _handlers.Value; }
        }

        public dynamic Get(Int32 id) {
            ContentItem ContentToView;
            Response rsp = new Response();
            if (id > 0) {
                List<ContentItem> li = _orchardServices.ContentManager.GetAllVersions(id).ToList();
                if (li.Count() == 0)
                    return _utilsServices.GetResponse(ResponseType.Validation, T("No content with this Id").ToString());
                else
                    if (li.Count() == 1)
                    ContentToView = li[0];
                else
                    ContentToView = _orchardServices.ContentManager.Get(id, VersionOptions.Latest);
                if (!_orchardServices.Authorizer.Authorize(OrchardCore.Contents.Permissions.ViewContent, ContentToView))
                    if (!_contentExtensionService.HasPermission(ContentToView.ContentType, Methods.Get, ContentToView))
                        return _utilsServices.GetResponse(ResponseType.UnAuthorized);
                if (((dynamic)ContentToView).AutoroutePart != null) {
                    string tenantname = "";
                    if (string.IsNullOrWhiteSpace(_shellSettings.RequestUrlPrefix) == false) {
                        tenantname = _shellSettings.RequestUrlPrefix + "/";
                    }
                    return Redirect(Url.Content("~/" + tenantname + "WebServices/Alias?displayAlias=" + ((dynamic)ContentToView).AutoroutePart.DisplayAlias));
                }
                else {
                    throw new Exception("Method not implemented, content without AutoroutePart");
                }
            }
            else
                return _utilsServices.GetResponse(ResponseType.None, T("No content with this Id").ToString());
        }

        /// <summary>
        /// esempio http://localhost/Laser.Orchard/expoincitta/api/Laser.Orchard.ContentExtension/Content?ContentType=User
        /// da richiamare come application/json e non come form
        /// </summary>
        /// <param name="ContentType"></param>
        /// <param name="Language"></param>
        /// <returns></returns>
        public dynamic Get(string ContentType, string Language = "it-IT") {
            ContentTypeDefinition contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(ContentType);
            if (contentTypeDefinition == null) {
                Response resp = new Response() {
                    Success = false,
                    Message = T("ContentType not exist").ToString(),
                    ErrorCode = ErrorCode.Validation
                };
                return resp;
            }
            var eObj = new ExpandoObject() as IDictionary<string, Object>;

            #region Tutti i field

            foreach (ContentTypePartDefinition ctpd in contentTypeDefinition.Parts) {
                var fields = ctpd.PartDefinition.Fields.ToList();
                string tipofield = "";
                foreach (ContentPartFieldDefinition singleField in fields) {
                    tipofield = singleField.FieldDefinition.Name;
                    if (tipofield == typeof(TaxonomyField).Name) {

                        #region Tassonomia in Lingua

                        var taxobase = _taxonomyService.GetTaxonomyByName(singleField.Settings["TaxonomyFieldSettings.Taxonomy"]);
                        int idmaster = taxobase.Id;
                        if (taxobase.ContentItem.As<LocalizationPart>() != null) {
                            if (((dynamic)taxobase.ContentItem).LocalizationPart.MasterContentItem != null) {
                                idmaster = ((dynamic)taxobase.ContentItem).LocalizationPart.MasterContentItem.Id;
                            }
                            if (((dynamic)taxobase.ContentItem).LocalizationPart.Culture != null) {
                                if (((dynamic)taxobase.ContentItem).LocalizationPart.Culture.Culture != Language) {
                                    taxobase = _taxonomyService.GetTaxonomies().Where(x => (x.Id == idmaster || (((dynamic)x.ContentItem).LocalizationPart.MasterContentItem != null && ((dynamic)x.ContentItem).LocalizationPart.MasterContentItem.Id == idmaster)) && ((dynamic)x.ContentItem).LocalizationPart.Culture.Culture == Language).FirstOrDefault();
                                }
                            }
                        }
                        List<TermPart> cata = _taxonomyService.GetTerms(taxobase.Id).ToList();//.GetTermsForContentItem(currentUser.Id, singleField.Name).ToList();
                        List<string> ListCategory = new List<string>();
                        ListCategory = cata.Select(x => x.Id.ToString()).ToList();
                        List<ElementDetail> elements = new List<ElementDetail>();
                        foreach (var term in taxobase.Terms) {
                            Int32? valore = term.Id;
                            Int32? mediaid = null;
                            try {
                                MediaLibraryPickerField mpf = (MediaLibraryPickerField)(term.Fields.Where(x => x.FieldDefinition.Name == "MediaLibraryPickerField").FirstOrDefault());
                                mediaid = mpf.Ids[0];
                            }
                            catch { }
                            if (!term.Selectable)
                                valore = null;
                            if (term.FullPath == "/" + term.Id.ToString() || term.FullPath == term.Id.ToString())
                                elements.Add(new ElementDetail() { Name = term.Name, Value = valore, ImageId = mediaid });
                            else {
                                Int32 idtermfather = Convert.ToInt32(term.FullPath.Split('/')[term.FullPath.Split('/').Length - 2]);
                                FindTaxoVM(elements, idtermfather).Children.Add(new ElementDetail() { Name = term.Name, Value = valore, ImageId = mediaid });
                            }
                        }
                        ResponseElement re = new ResponseElement();
                        bool solofoglie = Convert.ToBoolean(singleField.Settings["TaxonomyFieldSettings.LeavesOnly"]);
                        if (solofoglie) {
                            ElementDetail TempElement = new ElementDetail(); //elemento fittizzio per procedura ricorsiva
                            TempElement.Children = elements;
                            AnnullaNonFoglie(TempElement);
                            elements = TempElement.Children;
                        }
                        re.Values = elements;
                        re.Setting = new ResponseSetting { Type = "Taxonomie", Required = Convert.ToBoolean(singleField.Settings["TaxonomyFieldSettings.Required"]), SingleChoice = Convert.ToBoolean(singleField.Settings["TaxonomyFieldSettings.SingleChoice"]) };
                        eObj.Add(ctpd.PartDefinition.Name + "." + singleField.Name, re);

                        #endregion Tassonomia in Lingua
                    }
                    else
                        if (tipofield == typeof(EnumerationField).Name) {
                        string[] elencovalori = singleField.Settings["EnumerationFieldSettings.Options"].Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<string> elencoValoriInLingua = new List<string>();
                        List<ElementDetail> ele = new List<ElementDetail>();
                        foreach (string val in elencovalori) {
                            ElementDetail tvm = new ElementDetail();
                            tvm.Value = val;
                            tvm.Name = _localizedStringManager.GetLocalizedString("UserEnumeratore", val, Language);
                            ele.Add(tvm);
                        }
                        ResponseElement re = new ResponseElement();
                        re.Values = ele;
                        bool singlechoise = true;
                        if (singleField.Settings["EnumerationFieldSettings.ListMode"] == "Listbox" || singleField.Settings["EnumerationFieldSettings.ListMode"] == "Checkbox")
                            singlechoise = false;
                        re.Setting = new ResponseSetting { Type = "Enumerator", Required = Convert.ToBoolean(singleField.Settings["EnumerationFieldSettings.Required"]), SingleChoice = singlechoise };
                        eObj.Add(ctpd.PartDefinition.Name + "." + singleField.Name, re);
                    }
                }
            }

            #endregion Tutti i field

            return Json(eObj);
        }

        #region private class/method for get

        private void AnnullaNonFoglie(ElementDetail myelement) {
            if (myelement.Children.Count > 0)
                myelement.Value = null;
            foreach (ElementDetail el in myelement.Children) {
                AnnullaNonFoglie(el);
            }
        }

        private class ResponseElement {
            public object Values { get; set; }

            public ResponseSetting Setting { get; set; }
        }

        private class ResponseSetting {
            public string Type { get; set; }
            public bool Required { get; set; }
            public bool SingleChoice { get; set; }

            public ResponseSetting() {
                Required = false;
                SingleChoice = false;
            }
        }

        private class ElementDetail {

            public ElementDetail() {
                Name = "";
                Children = new List<ElementDetail>();
            }

            public string Name { get; set; }
            public object Value { get; set; }
            public Int32? ImageId { get; set; }
            public List<ElementDetail> Children { get; set; }
        }

        private ElementDetail FindTaxoVM(List<ElementDetail> elements, Int32 idToFind) {
            if (elements != null) {
                foreach (ElementDetail myterm in elements) {
                    if ((Int32)myterm.Value == idToFind)
                        return myterm;
                    else {
                        var foundinchildren = FindTaxoVM(myterm.Children, idToFind);
                        if (foundinchildren != null)
                            return FindTaxoVM(myterm.Children, idToFind);
                    }
                }
                return null;
            }
            else
                return null;
        }

        #endregion private class/method for get

        private Response DeleteContent(Int32 id) {
            ContentItem ContentToDelete;
            Response rsp = new Response();
            if (id > 0) {
                List<ContentItem> li = _orchardServices.ContentManager.GetAllVersions(id).ToList();
                if (li.Count() == 0)
                    return _utilsServices.GetResponse(ResponseType.Validation, T("No content with this Id").ToString());
                else
                    if (li.Count() == 1)
                    ContentToDelete = li[0];
                else
                    ContentToDelete = _orchardServices.ContentManager.Get(id, VersionOptions.Latest);
                if (!_orchardServices.Authorizer.Authorize(OrchardCore.Contents.Permissions.DeleteContent, ContentToDelete))
                    if (!_contentExtensionService.HasPermission(ContentToDelete.ContentType, Methods.Delete, ContentToDelete))
                        return _utilsServices.GetResponse(ResponseType.UnAuthorized);
                try {
                    _orchardServices.ContentManager.Remove(ContentToDelete);
                    // propaga l'evento Removed per il ContentItem
                    var context = new RemoveContentContext(ContentToDelete);
                    Handlers.Invoke(handler => handler.Removed(context), Logger);
                }
                catch (Exception ex) {
                    return _utilsServices.GetResponse(ResponseType.None, ex.Message);
                }
            }
            else
                return _utilsServices.GetResponse(ResponseType.None, T("No content with this Id").ToString());
            return (_utilsServices.GetResponse(ResponseType.Success));// { Message = "Invalid Token/csrfToken", Success = false, ErrorCode=ErrorCode.InvalidXSRF,ResolutionAction=ResolutionAction.Login });
        }

        /// <summary>
        /// http://localhost/Laser.Orchard/expoincitta/Api/Laser.Orchard.ContentExtension/ContentItem/2925
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Response Delete(Int32 id) {
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser == null)
                return DeleteContent(id);
            else
                if (_csrfTokenHelper.DoesCsrfTokenMatchAuthToken())
                return DeleteContent(id);
            else
                return (_utilsServices.GetResponse(ResponseType.InvalidXSRF));// { Message = "Invalid Token/csrfToken", Success = false, ErrorCode=ErrorCode.InvalidXSRF,ResolutionAction=ResolutionAction.Login });
        }

        /// <summary>
        /// http://localhost/Laser.Orchard/expoincitta/Api/Laser.Orchard.ContentExtension/ContentItem/2940
        /// </summary>
        /// <param name="id"></param>
        /// <param name="eObj"></param>
        /// <returns></returns>
        public Response Put(Int32 id, ExpandoObject eObj) {
            ((dynamic)eObj).Id = id;
            return Post(eObj);
        }

        /// <summary>
        /// test in feedler
        /// User-Agent: Fiddler
        ///Content-Type: application/json
        ///Host: localhost
        ///Content-Length: 105
        ///Cookie: .ASPXAUTH=3BDDACF3339764AE73D8AF6A9992CE0C34247D3107C7E66561BCF0A8ACB81C0B0708A274B5B5A57B3C9425AA98825AF5429AE0DE9958CEC8923B998667D89184755E3446DAA5832C7C16C519ABDD1981E919AC22E1A81D277F615F3240264D5FB0B46F174EEC84EB839387462EA250CC71B856178CE26EF0EDEFD6B00E40FACF
        ///X-XSRF-TOKEN: Tnaq+qFFu+B/NPrfJZnTg5FoATHDCrTP3aAAXA90MiS1vpkk5y2QwfRJ5aAmqu4n7GFv+6CcUos+klKlOXBu1A==
        ///{"ContentType":"contenutoutente","TitlePart.Title":"Titolodiprova","sottotitolo":"il mio sottotitolo","BodyPart.Text":"<b>il mio body</b>","media":[158] }
        /// </summary>
        /// <param name="eObj"></param>
        /// <returns></returns>
        [PolicyApiFilter]
        public Response Post(ExpandoObject eObj) {
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser == null)
                return StoreNewContentItem(eObj);
            else
                if (_csrfTokenHelper.DoesCsrfTokenMatchAuthToken()) {
                return StoreNewContentItem(eObj);
            }
            else
                return (_utilsServices.GetResponse(ResponseType.InvalidXSRF));// { Message = "Invalid Token/csrfToken", Success = false, ErrorCode=ErrorCode.InvalidXSRF,ResolutionAction=ResolutionAction.Login });
        }

        #region private method

        /// <summary>
        /// Formato DateTimeField: 2009-06-15T13:45:30  yyyy-MM-ddThh:mm:ss NB: L’ora deve essere riferita all’ora di Greenwich
        /// </summary>
        /// <param name="eObj"></param>
        /// <param name="TheContentItem"></param>
        /// <returns></returns>
        private Response StoreNewContentItem(ExpandoObject eObj) {
            // Reasoning on permissions will require us to know the type
            // of the content.
            string tipoContent = ((dynamic)eObj).ContentType;
            // We will also need to know the content's Id in case we are
            // trying to edit an existing ContentItem.
            Int32 IdContentToModify = 0; // new content
            try {
                if ((Int32)(((dynamic)eObj).Id) > 0) {
                    IdContentToModify = (Int32)(((dynamic)eObj).Id);
                }
            } catch {
                // Fix per Username nullo
                if (tipoContent == "User") {
                    return _utilsServices.GetResponse(ResponseType.Validation, "Missing user Id");
                }
            }
            // We will be doing a first check on the ContentType, to validate what's coming
            // to the API. The call to the GetTypeDefinition method will also do null checks
            // on the type name for us.
            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(tipoContent);
            if (typeDefinition == null) {
                // return an error of some sort here
                return _utilsServices.GetResponse(ResponseType.Validation, "Invalid ContentType");
            }
            // The ContentItem we will create/edit
            ContentItem NewOrModifiedContent;
            if (IdContentToModify == 0) {
                // We are going to be creating a new ContentItem
                NewOrModifiedContent = _contentManager.New(tipoContent);
                if (!_authorizer.Authorize(CorePermissions.CreateContent, NewOrModifiedContent)) {
                    // the user cannot create content of the given type, so
                    // return an error
                    return _utilsServices.GetResponse(ResponseType.UnAuthorized);
                }
                // since we may create, create
                _contentManager.Create(NewOrModifiedContent, VersionOptions.Draft);
            } else {
                // we are attempting to modify an existing items
                NewOrModifiedContent = _contentManager.Get(IdContentToModify, VersionOptions.DraftRequired);
            }
            if (NewOrModifiedContent == null) {
                // something went horribly wrong, so return an error
                return _utilsServices.GetResponse(ResponseType.Validation, "No content with this Id");
            }
            // If either of these validations fail, return an error because we cannot
            // edit the content
            // Validation 1: item should be of the given type
            if (NewOrModifiedContent.TypeDefinition.Name != tipoContent) {
                // return an error
                return _utilsServices.GetResponse(ResponseType.UnAuthorized);
            }
            // Validation 2: check EditContent Permissions
            if (!_authorizer.Authorize(CorePermissions.EditContent, NewOrModifiedContent)
                // we also check permissions that may exist for this specific method
                && !_contentExtensionService.HasPermission(tipoContent, Methods.Post)) {
                // return an error
                return _utilsServices.GetResponse(ResponseType.UnAuthorized);
            }
            // Validation 3: if we are also trying to publish, check PublishContent Permissions
            if (NewOrModifiedContent.Has<IPublishingControlAspect>()
                || NewOrModifiedContent.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable) {
                // in this case, simply the EditContent permission is not enough because that
                // would only allow the user to create a draftable
                if (!_authorizer.Authorize(CorePermissions.PublishContent, NewOrModifiedContent)) {
                    // return an error
                    return _utilsServices.GetResponse(ResponseType.UnAuthorized);
                }
            }
            // To summarize, here we have a valid ContentItem that we are authorized to edit
            Response rsp = new Response();
            // do some further custom validation
            string validateMessage = ValidateMessage(NewOrModifiedContent, IdContentToModify == 0 ? "Created" : "Modified");
            if (string.IsNullOrEmpty(validateMessage)) {
                // act like _contentManager.UpdateEditor
                var context = new UpdateContentContext(NewOrModifiedContent);
                // 1. invoke the Updating handlers
                Handlers.Invoke(handler => handler.Updating(context), Logger);
                // 2. do all the update operations
                rsp = _contentExtensionService.StoreInspectExpando(eObj, NewOrModifiedContent);
                if (rsp.Success) {
                    try {
                        string language = "";
                        try {
                            language = ((dynamic)eObj).Language;
                        } catch { }
                        if (NewOrModifiedContent.As<LocalizationPart>() != null) {
                            if (!string.IsNullOrEmpty(language))
                                NewOrModifiedContent.As<LocalizationPart>().Culture = _cultureManager.GetCultureByName(language);
                            NewOrModifiedContent.As<LocalizationPart>().MasterContentItem = NewOrModifiedContent;
                        }
                        validateMessage = ValidateMessage(NewOrModifiedContent, "");
                        if (string.IsNullOrEmpty(validateMessage) == false) {
                            rsp = _utilsServices.GetResponse(ResponseType.None, validateMessage);
                        }
                        if (NewOrModifiedContent.As<AutoroutePart>() != null) {
                            dynamic data = new ExpandoObject();
                            data.DisplayAlias = ((dynamic)NewOrModifiedContent).AutoroutePart.DisplayAlias;
                            data.Id = (Int32)(((dynamic)NewOrModifiedContent).Id);
                            data.ContentType = ((dynamic)NewOrModifiedContent).ContentType;
                            rsp.Data = data;
                        }
                    } catch (Exception ex) {
                        rsp = _utilsServices.GetResponse(ResponseType.None, ex.Message);
                    }
                }
                // 3. invoke the Updated handlers
                Handlers.Invoke(handler => handler.Updated(context), Logger);
                // Check whether any handler set some Error notifications (???)
                foreach (var notifi in _notifier.List()) {
                    if (notifi.Type == NotifyType.Error) {
                        // we'll cancel the transaction later
                        //_transactionManager.Cancel();
                        rsp.Success = false;
                        rsp.Message = "Error on update";
                        Logger.Error(notifi.Message.ToString());
                        break;
                    }
                }
            } else {
                // Custom validation failed
                // this one has by definition rsp.Success == false
                rsp = _utilsServices.GetResponse(ResponseType.None, validateMessage);
            }

            if (!rsp.Success) {
                // update failed
                _transactionManager.Cancel();
                // return an error
                return rsp;
            }


            // we want the ContentItem to be published, so it can be "seen" by mobile
            _contentManager.Publish(NewOrModifiedContent);

            return rsp;
        }

        private string ValidateMessage(ContentItem ci, string postfix) {
            string myfile = _razorService.CalculateFallbackTenantCodePosition("Validation", ci.ContentType + postfix + ".cshtml");
            var model = new RazorModelContext {
                OrchardServices = _orchardServices,
                ContentItem = ci,
                Tokens = new Dictionary<string, object>(),
                T = T
            };
            string result = _razorTemplateManager.RunFile(myfile, model);
            string resultnobr = result.Replace("\r\n", "").Replace(" ", "");
            if (!string.IsNullOrEmpty(resultnobr)) {
                return result;
            }
            return null;
        }
        #endregion private method
    }
}