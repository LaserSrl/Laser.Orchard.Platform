using Laser.Orchard.ContentExtension.ViewModels;
using Laser.Orchard.ContentExtension.Services;
using  Laser.Orchard.StartupConfig.Security;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using mlib= Orchard.MediaLibrary;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.UI.Notify;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.ContentExtension.Controllers {

    public class UploadFileController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _settings;
        private readonly IMediaLibraryService _mediaLibraryService;

        //  private readonly IContentManager _contentManager;
        private readonly IContentExtensionService _contentExtensionsServices;

        private readonly INotifier _notifier;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IUtilsServices _utilsServices;

        public Localizer T { get; set; }

        public UploadFileController(IOrchardServices orchardServices, ShellSettings settings, IMediaLibraryService mediaLibraryService, IContentExtensionService contentExtensionsServices, INotifier notifier, ICsrfTokenHelper csrfTokenHelper, IUtilsServices utilsServices) {
            _notifier = notifier;
            _contentExtensionsServices = contentExtensionsServices;
            _settings = settings;
            _orchardServices = orchardServices;
            _mediaLibraryService = mediaLibraryService;
            _utilsServices = utilsServices;
            T = NullLocalizer.Instance;
            _csrfTokenHelper = csrfTokenHelper;
        }

        [HttpGet]
        public ActionResult Index(string field, string filenumber, string folderfield, string subfolder, UploadFileVM filelist) {
            if (filelist.SubFolder == "")
                filelist.SubFolder = subfolder;
            if (filelist.FolderField == "")
                filelist.FolderField = folderfield;
            if (filelist.FileNumber == -1)
                filelist.FileNumber = Convert.ToInt32(filenumber);
            if (filelist.IdField == "")
                filelist.IdField = field;
            return View((object)filelist);
        }

        [HttpPost]
        public JsonResult PostFile(HttpPostedFileBase file, string contentType="") {
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser == null) {
                return PostFileFunction(file, contentType);
            }
            else
                if (_csrfTokenHelper.DoesCsrfTokenMatchAuthToken()) {
                    return PostFileFunction(file, contentType);
                }
                else {
                    return Json(_utilsServices.GetResponse(ResponseType.InvalidXSRF));// { Message = "Invalid Token/csrfToken", Success = false, ErrorCode=ErrorCode.InvalidXSRF,ResolutionAction=ResolutionAction.Login });
                }
        }

        private JsonResult PostFileFunction(HttpPostedFileBase file, string contentType = "") {
            if (!_orchardServices.Authorizer.Authorize(mlib.Permissions.ImportMediaContent)) {
                return Json(_utilsServices.GetResponse(ResponseType.UnAuthorized));
            }
            Int32 output = 0;
            string LinkFile = "/Media/" + _settings.Name + "/Upload";
            string pathString = Server.MapPath("~/Media/" + _settings.Name) + @"\Upload" + (contentType != "" ? "\\" + contentType : "");
            VerificaPath(pathString);
            string nomefile = System.IO.Path.GetFileName(file.FileName);
            if (_contentExtensionsServices.FileAllowed(nomefile)) {
                int contatore = 0;
                string cont = ""; 
                while (System.IO.File.Exists(System.IO.Path.Combine(pathString, cont + nomefile))) {
                    contatore++;
                    cont = contatore.ToString() + "_";
                }
                // file.SaveAs(System.IO.Path.Combine(pathString, cont + nomefile));

                //    var CIimage = _orchardServices.ContentManager.New("Image");
                MediaPart mediaPart = _mediaLibraryService.ImportMedia(file.InputStream, Path.GetFullPath(pathString).Replace(Server.MapPath("~/Media/" + _settings.Name + "/"), ""), cont + nomefile);
                // CIimage.As<MediaPart>()= mediaPart;
                var CIimage = _orchardServices.ContentManager.New(mediaPart.ContentItem.ContentType);
                _orchardServices.ContentManager.Create(mediaPart.ContentItem, VersionOptions.Published);
                //          filelist.ElencoUrl.Add(mediaPart.ContentItem.As<MediaPart>().Id);
                output = mediaPart.Id;
            }
            return Json(new { Id = output });
        }

        [HttpPost]
        public ActionResult Index(UploadFileVM filelist, HttpPostedFileBase file) {
            string linkFile = "/Media/" + _settings.Name;
            string pathString = Server.MapPath("~/Media/" + _settings.Name);
            //         pathString += @"\Upload";
            //         LinkFile += "/Upload";
            VerificaPath(pathString);
            if (filelist != null) {
                if (filelist.FolderField != "") {
                    pathString += @"\" + filelist.FolderField;
                    linkFile += "/" + filelist.FolderField;
                    VerificaPath(pathString);
                }

                if (filelist.SubFolder == "random") {
                    pathString += @"\" + Session.SessionID;
                    linkFile += @"/" + Session.SessionID;
                    VerificaPath(pathString);
                }
                else
                    if (_orchardServices.WorkContext.CurrentUser != null) {
                        if (filelist.SubFolder == "") {
                            string folder = _orchardServices.WorkContext.CurrentUser.Id.ToString();
                            pathString += @"\" + folder;   //pathString += @"\" + Session.SessionID + @"\";
                            linkFile += "/" + folder;  //LinkFile += @"\" + Session.SessionID + @"\";
                            VerificaPath(pathString);
                        }
                        else {
                            string folder = _orchardServices.WorkContext.CurrentUser.Id.ToString();
                            pathString += @"\" + filelist.SubFolder;   //pathString += @"\" + Session.SessionID + @"\";
                            linkFile += "/" + filelist.SubFolder;  //LinkFile += @"\" + Session.SessionID + @"\";
                            VerificaPath(pathString);
                        }
                    }
            }
            pathString += @"\";   //pathString += @"\" + Session.SessionID + @"\";
            linkFile += "/";
            string nomefile = System.IO.Path.GetFileName(file.FileName);
            if (_contentExtensionsServices.FileAllowed(nomefile)) {
                int contatore = 0;
                string cont = "";
                while (System.IO.File.Exists(System.IO.Path.Combine(pathString, cont + nomefile))) {
                    contatore++;
                    cont = contatore.ToString() + "_";
                }
                // file.SaveAs(System.IO.Path.Combine(pathString, cont + nomefile));

                //    var CIimage = _orchardServices.ContentManager.New("Image");
                MediaPart mediaPart = _mediaLibraryService.ImportMedia(file.InputStream, Path.GetFullPath(pathString).Replace(Server.MapPath("~/Media/" + _settings.Name + "/"), ""), cont + nomefile);
                // CIimage.As<MediaPart>()= mediaPart;
                var CIimage = _orchardServices.ContentManager.New(mediaPart.ContentItem.ContentType);

                _orchardServices.ContentManager.Create(mediaPart.ContentItem, VersionOptions.Published);
                //          filelist.ElencoUrl.Add(mediaPart.ContentItem.As<MediaPart>().Id);
                if (filelist != null) {
                    filelist.ElencoUrl.Add(linkFile.Replace("//", "/") + cont + nomefile);
                    filelist.ElencoId.Add(mediaPart.Id);
                }
            }
            else _notifier.Error(T("The file extension in the filename is not allowed or has not been provided."));

            return View(filelist);
        }

        private void VerificaPath(string stringpath) {
            if (!Directory.Exists(stringpath))
                Directory.CreateDirectory(stringpath);
        }
    }
}