using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.FileDownloader.ViewModels {
    public class FilesListVM {
        public List<FileSystemInfo> FileInfos { get; set; }
        public string FolderName { get; set; }
        public string ParentFolder { get; set; }
        public string FileFilter { get; set; }
        public string UrlBack { get; set; }
        public dynamic Pager { get; set; }
        public FilesListVM() {
            FileInfos = new List<FileSystemInfo>();
            FolderName = "";
            ParentFolder = "Export";
            FileFilter = "*";
        }
    }
}