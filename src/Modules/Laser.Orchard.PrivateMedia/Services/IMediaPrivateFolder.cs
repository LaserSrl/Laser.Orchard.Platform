using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.Media;

namespace Laser.Orchard.PrivateMedia.Services {
    public interface IMediaPrivateFolder : ISingletonDependency {
        bool IsPrivate(string path);
    }
    public class MediaPrivateFolder : IMediaPrivateFolder {

        private readonly ShellSettings _setting;

        private Dictionary<string, bool> PrivateFolders;
        private string _storagePath, _baseMediaUrl;


        public MediaPrivateFolder(ShellSettings setting) {
            PrivateFolders = new Dictionary<string, bool>();
            _setting = setting;
            var mediaPath = HostingEnvironment.IsHosted
                              ? HostingEnvironment.MapPath("~/Media/") ?? ""
                              : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media");
            _storagePath = (HostingEnvironment.IsHosted ? Path.Combine(mediaPath, _setting.Name) : Path.Combine(mediaPath, "Default")) + "\\";
            _baseMediaUrl = (HostingEnvironment.IsHosted ? "/Media/" + _setting.Name + "/" : "/Media/Default/");
            
        }

        public bool IsPrivate(string filePath) {
            if (string.IsNullOrEmpty(filePath))
                return TestFolder("\\");
            if (filePath.Contains(_baseMediaUrl)) {
                filePath = filePath.Substring(filePath.IndexOf(_baseMediaUrl) + _baseMediaUrl.Length - 1);
                filePath = filePath.Replace('/', '\\');
                return TestFolder(filePath.Substring(0, filePath.LastIndexOf('\\')));
            }
            else
                return false;
        }
        private bool TestFolder(string filePath) {
            if (!PrivateFolders.Keys.Contains(filePath)) {
                lock (string.Intern(filePath)) {
                    if (!PrivateFolders.Keys.Contains(filePath)) {
                        if (FolderExist(filePath)){
                            if (HasWebConfig(filePath))
                                PrivateFolders.Add(filePath, true);
                            else {
                                if (filePath.IndexOf('\\') >= 0)
                                    PrivateFolders.Add(filePath, TestFolder(filePath.Substring(0, filePath.LastIndexOf('\\'))));
                                else
                                    PrivateFolders.Add(filePath, false);
                            }
                        }
                        else
                            return false;
                    }
                    else
                        return PrivateFolders[filePath];
                }
            }
            return PrivateFolders[filePath];
        }
        private bool FolderExist(string mediapath) {
                return (Directory.Exists(Path.GetFullPath(_storagePath + mediapath)));
        }
        private bool HasWebConfig(string mediapath) {
            if (mediapath.Equals(""))
                return (File.Exists(Path.GetFullPath(_storagePath + mediapath) + "Web.config"));
            else
                return (File.Exists(Path.GetFullPath(_storagePath + mediapath) + "\\Web.config"));
        }

    }
}