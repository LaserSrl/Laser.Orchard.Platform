using Laser.Orchard.StartupConfig.FileDownloader.ViewModels;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Security.Permissions;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.FileDownloader.Controllers {
    /// <summary>
    /// Gestisce il download di file salvati in una struttura di cartelle sotto App_Data/Sites/NomeDelTenant.
    /// La struttura è ParentFolder/FolderName/FileFilter.
    /// Se non diversamente specificato ParentFolder = Export.
    /// Se non diversamente specificato FileFilter = * (tutti i file).
    /// E' necessario specificare l'URL di ritorno (UrlBack).
    /// Il controllo accessi avviene tramite il check sul permesso "AccessParentFolderFolterName": ad esempio nel caso in cui 
    /// non sia specificato ParentFolder e il FolderName sia QuestionnairesStatistics, il controllo avviene sul permesso AccessExportQuestionnairesStatistics.
    /// Esempio di utilizzo in un hyperlink:
    /// 
    /// href="@Url.Action("Index", "FileDownloader", new { area = "Laser.Orchard.StartupConfig", UrlBack = urlBack })&FolderName=Contacts&ParentFolder=Import&FileFilter=*.log"
    /// 
    /// In questo esempio verranno mostrati i file ~/App_Data/Sites/Tenant/Import/Contacts/*.log.
    /// </summary>
    public class FileDownloaderController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly INotifier _notifier;
        private readonly ShellSettings _shellSettings;
        public Localizer T { get; set; }
        private string FileRootRelativePath {
            get {
                return string.Format("~/App_Data/Sites/{0}", _shellSettings.Name);
            }
        }

        public FileDownloaderController(IOrchardServices orchardServices, INotifier notifier, ShellSettings shellSettings) {
            _orchardServices = orchardServices;
            _notifier = notifier;
            _shellSettings = shellSettings;
            T = NullLocalizer.Instance;
        }
        private bool CheckPermission(string folderName, string parentFolder) {
            var accessPermission = Permission.Named(string.Format("Access{0}{1}", parentFolder, folderName));
            return _orchardServices.Authorizer.Authorize(accessPermission);
        }
        [HttpGet]
        [Admin]
        public ActionResult Index(FilesListVM model, PagerParameters pagerParameters) {
            // Check user permission
            if (CheckPermission(model.FolderName, model.ParentFolder) == false) {
                return new HttpUnauthorizedResult();
            }
            // Check if folders exist
            var folderExists = true;
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Server.MapPath(FileRootRelativePath + "/" + model.ParentFolder));
            folderExists = dir.Exists;
            if (folderExists) {
                dir = new System.IO.DirectoryInfo(Server.MapPath(FileRootRelativePath + "/" + model.ParentFolder + "/" + model.FolderName));
                folderExists = dir.Exists;
            }
            if (folderExists) {
                var files = dir.GetFiles(model.FileFilter, System.IO.SearchOption.TopDirectoryOnly);
                foreach (var file in files) {
                    model.FileInfos.Add(file);
                }
                model.FileInfos = model.FileInfos.OrderByDescending(x => x.LastWriteTimeUtc).ToList();
            } else {
                model.FileInfos = new List<FileSystemInfo>();
            }

            // Paging
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(model.FileInfos.Count);
            model.FileInfos = model.FileInfos.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();
            model.Pager = pagerShape;
            return View("Index", model);
        }
        public ActionResult DownloadFile(string fName, string folderName, string parentFolder = "Export") {
            // Check user permission
            if (CheckPermission(folderName, parentFolder) == false) {
                return new HttpUnauthorizedResult();
            }
            var fPath = Server.MapPath(FileRootRelativePath + "/" + parentFolder + "/" + folderName + "/" + fName);
            byte[] buffer = System.IO.File.ReadAllBytes(fPath);
            return File(buffer, "application/octet-stream", fName);
        }
        [Admin]
        public ActionResult RemoveFile(string fName, string folderName, string urlBack, string parentFolder = "Export") {
            // Check user permission
            if (CheckPermission(folderName, parentFolder) == false) {
                return new HttpUnauthorizedResult();
            }
            var fPath = Server.MapPath(FileRootRelativePath + "/" + parentFolder + "/" + folderName + "/" + fName);
            System.IO.FileInfo file = new System.IO.FileInfo(fPath);
            if (file.Exists) {
                file.Delete();
                _notifier.Information(T("File removed."));
            } else {
                _notifier.Error(T("File does not exist. It should have been removed by someone else."));
            }
            return RedirectToAction("Index", new { UrlBack = urlBack, FolderName = folderName, ParentFolder = parentFolder });
        }
    }
}