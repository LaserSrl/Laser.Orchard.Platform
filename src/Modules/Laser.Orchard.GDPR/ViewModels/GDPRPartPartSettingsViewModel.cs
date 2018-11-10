using Laser.Orchard.GDPR.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.GDPR.ViewModels {
    public class GDPRPartPartSettingsViewModel {

        public GDPRPartPartSettingsViewModel() {
            Settings = new GDPRPartPartSettings();
        }

        public GDPRPartPartSettings Settings { get; set; }

        public bool ShouldAnonymize {
            get { return Settings.ShouldAnonymize; }
            set { Settings.ShouldAnonymize = value; }
        }

        public bool ShouldErase {
            get { return Settings.ShouldErase; }
            set { Settings.ShouldErase = value; }
        }

        public string ErasurePropertyValuePairs {
            get { return JsonConvert.SerializeObject(Settings.ErasurePropertyValuePairs); }
            set {
                if (string.IsNullOrWhiteSpace(value)) {
                    Settings.ErasurePropertyValuePairs = new Dictionary<string, string>();
                } else {
                    try {
                        Settings.ErasurePropertyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
                    } catch (Exception ex) {
                        throw ex;
                    }
                }
            }
        }

        public string AnonymizationPropertyValuePairs {
            get { return JsonConvert.SerializeObject(Settings.AnonymizationPropertyValuePairs); }
            set {
                if (string.IsNullOrWhiteSpace(value)) {
                    Settings.AnonymizationPropertyValuePairs = new Dictionary<string, string>();
                } else {
                    try {
                        Settings.AnonymizationPropertyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
                    } catch (Exception ex) {
                        throw ex;
                    }
                }
            }
        }
    }
}