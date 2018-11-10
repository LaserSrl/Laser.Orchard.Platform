using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.CommunicationGateway.Utils {
    public class DownloadFileContentResult : FileContentResult {
        private string _fileSystemPath;
        private string _fName;

        public DownloadFileContentResult(string fileSystemPath, string fName, string contentType)
            : base(new byte[0], contentType) {
            _fileSystemPath = fileSystemPath;
            _fName = fName;
        }

        protected override void WriteFile(HttpResponseBase response) {
            response.Clear();
            response.AppendHeader("content-disposition", "attachment; filename=" + _fName);
            response.ContentType = "application/octet-stream;charset=utf-8";
            response.TransmitFile(_fileSystemPath);
            //response.WriteFile(_fileSystemPath);
            //byte[] buffer = System.IO.File.ReadAllBytes(_fileSystemPath);
            //string check = Encoding.UTF8.GetString(buffer);
            //response.BinaryWrite(buffer);
        }
    }
}