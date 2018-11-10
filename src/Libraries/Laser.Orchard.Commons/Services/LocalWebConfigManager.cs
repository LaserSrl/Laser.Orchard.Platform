using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Laser.Orchard.Commons.Services {
    public class LocalWebConfigManager {

        public System.Configuration.Configuration GetConfiguration(string url) {
            var fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = HostingEnvironment.MapPath(url + "/web.config");
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            return configuration;
        }

        /// <summary>
        ///  
        ///   
        /// esempio LeggiConfigSetting("Laser.Orchard.MioModulo","Masterkey","ModuleConfig")
        /// cercherà nel webconfig di Laser.Orchard.MioModulo  il valore sel setting "MasterKeY" nel setting group ModuleConfig 
        /// 
        ///  esempio LeggiConfigSetting("Laser.Orchard.MioModulo","Masterkey",null)
        ///  cerca in appsetting
        /// </summary>
        /// <param name="module"></param>
        /// <param name="appsetting"></param>
        /// <param name="section"> se null il default è "ModuleConfig"</param>
        /// <returns></returns>
        public string GetConfigSetting(string module, string appsetting, string section) {
            var conf = GetConfiguration("~/Modules/" + module);
            string theconfig = "";
            if (string.IsNullOrEmpty(section)) {
                theconfig = conf.AppSettings.Settings[appsetting].Value;
                //   section = "ModuleConfig";
            } else {
                var settings = conf.GetSection(section) as AppSettingsSection;
                if (settings != null)
                    theconfig = settings.Settings[appsetting].Value;
            }
            return theconfig;
        }


    }
}