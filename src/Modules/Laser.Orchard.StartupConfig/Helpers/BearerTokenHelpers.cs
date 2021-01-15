using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Helpers {
    public static class BearerTokenHelpers {

        #region Serialization of UserData Dictionary
        // Use Newtonsoft.Json to handle this
        public static string SerializeUserDataDictionary(IDictionary<string, string> userDataDictionary) {
            return JsonConvert.SerializeObject(userDataDictionary, Formatting.None);
        }

        public static  Dictionary<string, string> DeserializeUserData(string userData) {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(userData);
        }

        #endregion
    }
}