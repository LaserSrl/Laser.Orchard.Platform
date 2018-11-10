using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Laser.Orchard.StartupConfig.WebApiProtection.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.AppData;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IApiKeySettingService : ISingletonDependency {
        string EncryptionKeys(string key);
        void Refresh();
    }
    public class ApiKeySettingService : IApiKeySettingService {
        private Dictionary<string, string> _encryptionKeys;
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private readonly IAppDataFolder _appDataFolder;
        private string _filePath;
        private DateTime _lastWriteTimedUtc;
        public ApiKeySettingService(IOrchardServices orchardServices, ShellSettings shellSettings, IAppDataFolder appDataFolder) {
            _shellSettings = shellSettings;
            _appDataFolder = appDataFolder;
            _orchardServices = orchardServices;
            _filePath = Path.Combine(Path.Combine("Sites", _shellSettings.Name), "ApiSetting.txt");
            ReadFileSetting();
        }
        private string ReadFileSetting() {
            var content = "";
            _encryptionKeys = new Dictionary<string, string>();
            if (!_appDataFolder.FileExists(_filePath)) {
                var key = "";
                content = "TheDefaultChannel" + ":" + key + Environment.NewLine;
                _encryptionKeys.Add("TheDefaultChannel", key);
                _appDataFolder.CreateFile(_filePath, content);
            }
            else {
                var filecontent = _appDataFolder.ReadFile(_filePath);
                var lines = filecontent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines) {
                    var keyval = line.Split(':');
                    if (keyval.Length > 1)
                        if (!string.IsNullOrEmpty(line.Substring(keyval[0].Length + 1).Trim())) {
                            _encryptionKeys.Add(keyval[0], line.Substring(keyval[0].Length + 1).Trim());
                        }
                        else {
                            _encryptionKeys.Add(keyval[0], "");
                        }
                }
                content = filecontent;
            }
            _lastWriteTimedUtc = _appDataFolder.GetFileLastWriteTimeUtc(_filePath);
            return content;
        }
        public void Refresh() {
            var content = ReadFileSetting();
            var savefile = false;
            var settings = _orchardServices.WorkContext.CurrentSite.As<ProtectionSettingsPart>();
            var builder = new System.Text.StringBuilder();
            builder.Append(content);
            foreach (var set in settings.ExternalApplicationList.ExternalApplications) {
                if (!_encryptionKeys.Keys.Contains(set.Name)) {
                    savefile = true;
                    var key = "";
                    builder.Append(set.Name + ":" + key + Environment.NewLine);
                }
            }
            content = builder.ToString();
            if (savefile) {
                _appDataFolder.CreateFile(_filePath, content);
                ReadFileSetting();
            }
        }
        public string EncryptionKeys(string key) {
            if (_lastWriteTimedUtc != _appDataFolder.GetFileLastWriteTimeUtc(_filePath)) {
                ReadFileSetting();
            }
            var encryptionKeyValue = _shellSettings.EncryptionKey; //default value
            if (!string.IsNullOrEmpty(key) && _encryptionKeys.Keys.Contains(key) && !string.IsNullOrWhiteSpace(_encryptionKeys[key])) { //if exists the specific key value
                encryptionKeyValue = _encryptionKeys[key];
            }
            else if (_encryptionKeys.Keys.Contains("TheDefaultChannel") && !string.IsNullOrWhiteSpace(_encryptionKeys["TheDefaultChannel"])) { //fallback if the key is missing and exists a "TheDefaultChannel" key/value
                encryptionKeyValue = _encryptionKeys["TheDefaultChannel"];
            }
            return encryptionKeyValue;
        }
    }
}