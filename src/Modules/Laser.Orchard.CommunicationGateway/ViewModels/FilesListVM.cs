using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Laser.Orchard.CommunicationGateway.ViewModels {
    public class FilesListVM {
        public List<FileSystemInfo> FileInfos { get; set; }
        public string FilePath { get; set; }

        public FilesListVM() {
            FileInfos = new List<FileSystemInfo>();
            FilePath = "";
        }
    }
}