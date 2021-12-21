using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Laser.Orchard.Commons.Services {
    public class CommonUtils {
        public string NormalizeFileName(string fileName, string defaultFileName, char replacementChar = '_') {
            var normalizedFileName = defaultFileName;
            var tempFileName = fileName.Clone().ToString();
            // if the input character is one of the invalid characters
            if (Path.GetInvalidFileNameChars().Contains(replacementChar)) {
                // replaced it with the default one
                replacementChar = '_';
            }
            foreach (var badChar in Path.GetInvalidFileNameChars()) {
                tempFileName = tempFileName.Replace(badChar, replacementChar);
            }
            if (string.IsNullOrWhiteSpace(tempFileName.Trim()) == false) {
                normalizedFileName = tempFileName;
            }
            return normalizedFileName;
        }
        public byte[] CreateZipArchive(IEnumerable<byte[]> fileContents, IEnumerable<string> fileNames, string fileExtension) {
            byte[] zip = new byte[0];
            var addedNames = new List<string>();
            using (var memoryStream = new MemoryStream()) {
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true)) {
                    var fileNamesEnumerator = fileNames.GetEnumerator();
                    foreach (var content in fileContents) {
                        // get file name
                        var fileName = "";
                        if (fileNamesEnumerator.MoveNext()) {
                            fileName = fileNamesEnumerator.Current;
                        }
                        fileName = NormalizeFileName(fileName, "file");
                        // avoid duplicate file names
                        var suffix = 0;
                        var radix = fileName;
                        while (addedNames.Contains(fileName)) {
                            suffix++;
                            fileName = radix + suffix.ToString();
                        }
                        // add file to zip archive
                        var file = archive.CreateEntry(fileName + "." + fileExtension);
                        using (var writer = file.Open()) {
                            writer.Write(content, 0, content.Length);
                        }
                    }
                }
                zip = memoryStream.ToArray();
            }
            return zip;
        }
        public byte[] CreateZipArchive(IEnumerable<string> textContents, IEnumerable<string> fileNames, string fileExtension) {
            var binaryContents = new List<byte[]>();
            foreach(var text in textContents) {
                binaryContents.Add(Encoding.UTF8.GetBytes(text));
            }
            return CreateZipArchive(binaryContents, fileNames, fileExtension);
        }
    }
}