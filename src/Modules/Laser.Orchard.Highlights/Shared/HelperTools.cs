using System;
using System.Configuration;
using System.Web;

namespace Laser.Orchard.Highlights.Shared {
	
	public static class HelperTools {

		# region [ PROPRIETA' STATICHE ]

        private static ExeConfigurationFileMap _exeConfigFilename;

		/// <summary>
        /// Valore proveniente dal web.config -  Velocità di passaggio fra le immagini del Minibanner
		/// </summary>
        public static int WC_mbTransitionSpeed
        {
			get {
                var settingValue = GetCustomSettingValue("MiniBanner:TransitionSpeed");		
				return(
					(settingValue != null) 
						? Convert.ToInt32(settingValue) 
						: 900);
			} 
		}

        /// <summary>
        /// Valore proveniente dal web.config - Velocità di attesa sulla singola immagine del Minibanner
        /// </summary>
        public static int WC_mbTimeOut
        {
            get
            {
                var settingValue = GetCustomSettingValue("MiniBanner:TimeOut");
                return (
					(settingValue != null) 
						? Convert.ToInt32(settingValue) 
						: 5000);
            }
        }


        /// <summary>
        /// Valore proveniente dal web.config -  Velocità di passaggio fra le immagini del MidiBanner
        /// </summary>
        public static int WC_midiTransitionSpeed
        {
            get
            {
                var settingValue = GetCustomSettingValue("MidiBanner:TransitionSpeed");
                return (
                    (settingValue != null)
                        ? Convert.ToInt32(settingValue)
                        : 900);
            }
        }

        /// <summary>
        /// Valore proveniente dal web.config - Velocità di attesa sulla singola immagine del MidiBanner
        /// </summary>
        public static int WC_midiTimeOut
        {
            get
            {
                var settingValue = GetCustomSettingValue("MidiBanner:TimeOut");
                return (
                    (settingValue != null)
                        ? Convert.ToInt32(settingValue)
                        : 5000);
            }
        }

		// ......................................................
				
		/// <summary>
		/// Recupera, dal file di configurazione '~/Config/Laser.Orchard.Highlights.config', i valori di configurazione indicati attraverso la chiave
		/// </summary>
		/// <param name="appSettingsKey">Chiave di riferimento</param>
		private static string GetCustomSettingValue(string appSettingsKey) {

            KeyValueConfigurationElement setting;

			if (_exeConfigFilename == null) {
				_exeConfigFilename = 
					new ExeConfigurationFileMap {
                        ExeConfigFilename = HttpContext.Current.Server.MapPath("~/Config/Laser.Orchard.Highlights.config")
					};
			}

			if (!System.IO.File.Exists(_exeConfigFilename.ExeConfigFilename))
				throw new Exception(string.Format("[HelperTools.GetCustomSettingValue]: Impossibile trovare il file di configurazione \"{0}\".", _exeConfigFilename.ExeConfigFilename));
            else
            {
			    var configurations = 
				    ConfigurationManager.OpenMappedExeConfiguration(
					    _exeConfigFilename, 
					    ConfigurationUserLevel.None
				    );

			     setting = configurations.AppSettings.Settings[appSettingsKey];
            }
			return ((setting != null)
					      ? setting.Value
					      : null);
			
		}
				
		# endregion [ PROPRIETA' STATICHE ]

	}
}