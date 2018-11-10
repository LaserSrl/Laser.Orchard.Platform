using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.Security;
using RazorEngine;
using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Http;
using OrchardCore = Orchard.Core;
using System.Linq;


namespace Laser.Orchard.ContentExtension.Controllers {
    public class sasa {

        public string Language { get; set; }
        public string ContentType { get; set; }
        public string Valorenumerico { get; set; }

    }
    public class testpostController : ApiController {
        private readonly IAuthenticationService _authenticationService;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IOrchardServices _orchardServices;
        private readonly Lazy<IAutorouteService> _autorouteService;
        private readonly ILocalizationService _localizationService;
        private readonly ICultureManager _cultureManager;
        private readonly ShellSettings _shellSettings;

        public ILogger Logger { get; set; }

        //    private readonly ITaxonomyService _taxonomyService;
        private readonly IContentExtensionsServices _contentExtensionsServices;

        private readonly IUtilsServices _utilsServices;

        public Localizer T { get; set; }

        //       private readonly ILocalizedStringManager _localizedStringManager;

        //     public ILogger Log { get; set; }

        public testpostController(
            ShellSettings shellSettings,
            ICsrfTokenHelper csrfTokenHelper,
            IOrchardServices orchardServices,
            //    ITaxonomyService taxonomyService,
            //    ILocalizedStringManager localizedStringManager,
            IAuthenticationService authenticationService,
            IContentExtensionsServices contentExtensionsServices,
            Lazy<IAutorouteService> autorouteService,
            ILocalizationService localizationService,
            ICultureManager cultureManager,
            IUtilsServices utilsServices
            ) {
            _shellSettings = shellSettings;
            _csrfTokenHelper = csrfTokenHelper;
            _orchardServices = orchardServices;
            //      _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
            //     _localizedStringManager = localizedStringManager;
            //       Log = NullLogger.Instance;
            _authenticationService = authenticationService;
            _contentExtensionsServices = contentExtensionsServices;
            _autorouteService = autorouteService;
            _localizationService = localizationService;
            _cultureManager = cultureManager;
            _utilsServices = utilsServices;

            //  _context = context;
            Logger = NullLogger.Instance;
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
        public Response Post(sasa eObjs) {
            dynamic eObj = new ExpandoObject();
           // var eObj = new ExpandoObject() as IDictionary<string, Object>;
            //eObj.Add("Language", eObjs.Language);
            //eObj.Add("ContentType", eObjs.ContentType);
            //eObj.Add("Valorenumerico", eObjs.Valorenumerico);
            //eObj.Add("TitlePart.Title", "titolo");
            eObj.Language= eObjs.Language;
            eObj.ContentType= eObjs.ContentType;
            eObj.Valorenumerico= eObjs.Valorenumerico;
 
            //         try {
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser == null) {
                return StoreNewContentItem(eObj, null);
                //return (_utilsServices.GetResponse(ResponseType.InvalidUser));
            }
            else

                if (_csrfTokenHelper.DoesCsrfTokenMatchAuthToken()) {
                    return StoreNewContentItem(eObj, currentUser.ContentItem);
                }
                else {
                    //if (!_orchardServices.Authorizer.Authorize(OrchardCore.Settings.Permissions.ManageSettings, T("You don't have permission \"Manage settings\" to define and manage User Groups!"))) {
                    //}
                    //else {
                    //    return new HttpUnauthorizedResult();
                    //}
                    return (_utilsServices.GetResponse(ResponseType.InvalidXSRF));// { Message = "Invalid Token/csrfToken", Success = false, ErrorCode=ErrorCode.InvalidXSRF,ResolutionAction=ResolutionAction.Login });
                }
        }

        private Response StoreNewContentItem(ExpandoObject eObj, ContentItem TheContentItem) {
            string tipoContent = ((dynamic)eObj).ContentType;
            ContentItem NewContent = _orchardServices.ContentManager.New(tipoContent);
            if (!_orchardServices.Authorizer.Authorize(OrchardCore.Contents.Permissions.EditContent, NewContent)) {
                return _utilsServices.GetResponse(ResponseType.UnAuthorized);
            }

            // NewContent.As<TitlePart>.Title = "Creazione";
            _orchardServices.ContentManager.Create(NewContent, VersionOptions.Draft);// se non faccio il create poi non vengono salvati i field
          Response rsp =new Response();
            string validateMessage=ValidateMessage(NewContent,"Created");
            if (string.IsNullOrEmpty(validateMessage)){
               rsp = _contentExtensionsServices.StoreInspectExpando(eObj, NewContent);
          }
          else {
              rsp = _utilsServices.GetResponse(ResponseType.None, validateMessage);
            }
           
            if (rsp.Success) {
                try {
                    string language = "";
                    try {
                        language = ((dynamic)eObj).Language;
                    }
                    catch { }
                    if (NewContent.As<LocalizationPart>() != null) {
                        if (!string.IsNullOrEmpty(language))
                            NewContent.As<LocalizationPart>().Culture = _cultureManager.GetCultureByName(language);
                        NewContent.As<LocalizationPart>().MasterContentItem = NewContent;
                    }
                    validateMessage = ValidateMessage(NewContent, "");
                    if (string.IsNullOrEmpty(validateMessage)) {
                        _orchardServices.ContentManager.Create(NewContent, VersionOptions.Draft);
                    }
                    else {
                        rsp=_utilsServices.GetResponse(ResponseType.None, validateMessage);
                    }

                   

                    // _localizationService.SetContentCulture(NewContent, language);
                    if (((dynamic)NewContent).AutoroutePart != null) {
                        ((dynamic)NewContent).AutoroutePart.DisplayAlias = _autorouteService.Value.GenerateAlias(((dynamic)NewContent).AutoroutePart);
                        _autorouteService.Value.ProcessPath(((dynamic)NewContent).AutoroutePart);
                        _autorouteService.Value.PublishAlias(((dynamic)NewContent).AutoroutePart);
                        dynamic data = new ExpandoObject();
                        data.DisplayAlias = ((dynamic)NewContent).AutoroutePart.DisplayAlias;
                        rsp.Data = data;
                    }
                    //   Handlers.Invoke(handler => handler.Updating(context), Logger);
                }
                catch (Exception ex) {
                    try {
                        _orchardServices.ContentManager.Remove(NewContent);
                    }
                    catch (Exception ex2) {
                        rsp = _utilsServices.GetResponse(ResponseType.None, ex2.Message);
                    }
                    rsp = _utilsServices.GetResponse(ResponseType.None, ex.Message);
                }
            }
            else {
                try {
                    _orchardServices.ContentManager.Remove(NewContent);
                }
                catch (Exception ex2) {
                    rsp = _utilsServices.GetResponse(ResponseType.None, ex2.Message);
                }
            }
            //if (this.ExternalContentCreated != null) {
            //    this.ExternalContentCreated(this, new EventArgs());
            //}

            return rsp;
        }

        //public event EventHandler ExternalContentCreated;
        private string ValidateMessage(ContentItem ci, string postfix) {
            string validate_folder = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSettings.Name + @"\Validation\";
            if (!System.IO.Directory.Exists(validate_folder))
                System.IO.Directory.CreateDirectory(validate_folder);
            string myfile = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSettings.Name + @"\Validation\" + ci.ContentType +postfix+ ".cshtml";
            if (System.IO.File.Exists(myfile)) {
                string mytemplate = File.ReadAllText(myfile);
                if (!string.IsNullOrEmpty(mytemplate)) {
                    var config = new TemplateServiceConfiguration();
                    string result = "";
                    using (var service = RazorEngineService.Create(config)) {
                        result = service.RunCompile(mytemplate, "htmlRawTemplatea",null, (Object)ci);
                    }
                    string resultnobr = result.Replace("\r\n", "").Replace(" ", "");
                   if (!string.IsNullOrEmpty(resultnobr)) {
                        return result;
                    }
                }

            }
            return null;
        }

        private bool TryValidate(ContentItem ci) {
            //var context = new ValidationContext(ci.Parts.FirstOrDefault(), serviceProvider: null, items: null);
            //var results = new List<ValidationResult>();
            //var isValid = Validator.TryValidateObject(ci.Parts.FirstOrDefault(), context, results);
            //if (!isValid)
            //    return false;
            //else
            return true;
        }
    }
    //class MyIReferenceResolver : IReferenceResolver {
    //    //public string FindLoaded(IEnumerable<string> refs, string find) {
    //    //    return refs.First(r => r.EndsWith(System.IO.Path.DirectorySeparatorChar + find));
    //    //}
    //    public IEnumerable<CompilerReference> GetReferences(TypeContext context, IEnumerable<CompilerReference> includeAssemblies) {
    //        return new[]{
    //             CompilerReference.From(HostingEnvironment.MapPath("~/")+  @"bin\Orchard.Framework.dll")
    //            //CompilerReference.From(HostingEnvironment.MapPath("~/")+  @"App_Data\Dependencies\Orchard.dll")
    //                    };
    //        // TypeContext gives you some context for the compilation (which templates, which namespaces and types)

    //        // You must make sure to include all libraries that are required!
    //        // Mono compiler does add more standard references than csc! 
    //        // If you want mono compatibility include ALL references here, including mscorlib!
    //        // If you include mscorlib here the compiler is called with /nostdlib.
    //        //IEnumerable<string> loadedAssemblies = (new UseCurrentAssembliesReferenceResolver())
    //        //    .GetReferences(context, includeAssemblies)
    //        //    .Select(r => r.GetFile())
    //        //    .ToArray();

    //    //    yield return CompilerReference.From(FindLoaded(loadedAssemblies, "mscorlib.dll"));
    //    //    yield return CompilerReference.From(FindLoaded(loadedAssemblies, "System.dll"));
    //  //      yield return CompilerReference.From(FindLoaded(loadedAssemblies, "System.Core.dll"));
    //   //     yield return CompilerReference.From(typeof(MyIReferenceResolver).Assembly); // Assembly

    //        // There are several ways to load an assembly:
    //        //yield return CompilerReference.From("Path-to-my-custom-assembly"); // file path (string)
    //        //byte[] assemblyInByteArray = --- Load your assembly ---;
    //        //yield return CompilerReference.From(assemblyInByteArray); // byte array (roslyn only)
    //        //string assemblyFile = --- Get the path to the assembly ---;
    //        //yield return CompilerReference.From(File.OpenRead(assemblyFile)); // stream (roslyn only)
    //    }
    //}
}