using Laser.Orchard.StartupConfig.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Laser.Orchard.StartupConfig.RazorBase.Services {
    public interface IRazorBaseService : IDependency {
        /// <summary>
        /// Calculates the file path to execute searching it with fallback logics. see: EnvironmentVariablesSettingsPart.
        /// </summary>
        /// <param name="postTenantFolder">All razor files are by default searched within the folder /App_Data/Sites/{tenant}/{codefolder}/: insert the string to substitute {codefolder} placeholder</param>
        /// <param name="codeFileNameWithExtension">The Razor FileName</param>
        /// <returns></returns>
        string CalculateFallbackTenantCodePosition(string postTenantFolder, string codeFileNameWithExtension);
    }

    public class RazorBaseService : IRazorBaseService {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private const string URIFORMAT = "~/App_Data/Sites/{0}/{1}";

        public RazorBaseService(ShellSettings shellSettings, IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            _shellSettings = shellSettings;

        }
        public string CalculateFallbackTenantCodePosition(string postTenantFolder, string codeFileNameWithExtension) {
            var uriDir = String.Format(URIFORMAT, _shellSettings.Name, postTenantFolder);
            var uriFile = String.Format("{0}/{1}", uriDir, codeFileNameWithExtension);
            var localDir = HostingEnvironment.MapPath(uriDir);
            string localFile;
            var razorFileFound = false;
            if (!System.IO.Directory.Exists(localDir))
                System.IO.Directory.CreateDirectory(localDir);
            localFile = HostingEnvironment.MapPath(uriFile);
            if (!System.IO.File.Exists(localFile)) {
                var envVariablesSettingsPart = _orchardServices.WorkContext.CurrentSite.As<EnvironmentVariablesSettingsPart>();
                var fallbackTenants = new string[] { };
                if (envVariablesSettingsPart != null) {
                    if (string.IsNullOrWhiteSpace(envVariablesSettingsPart.FallbackTenants) == false) {
                        fallbackTenants = envVariablesSettingsPart.FallbackTenants.Split(';');
                    }
                }
                foreach (var tenant in fallbackTenants) {
                    uriFile = String.Format("{0}/{1}", String.Format(URIFORMAT, tenant, postTenantFolder), codeFileNameWithExtension);
                    localFile = HostingEnvironment.MapPath(uriFile);
                    if (System.IO.File.Exists(localFile)) {
                        razorFileFound = true;
                        break;
                    }
                }
            } else {
                razorFileFound = true;
            }
            if (!razorFileFound) {
                uriFile = String.Format("{0}/{1}", String.Format("~/App_Data/Shared/Razors/{0}", postTenantFolder), codeFileNameWithExtension);
                localFile = HostingEnvironment.MapPath(uriFile);
            }
            return localFile;
        }
    }
}