using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Laser.Orchard.StartupConfig.WebApiProtection.Models;
using Orchard.ContentManagement;
using Orchard.Utility.Extensions;
using System.Text;
using Orchard.Environment.Extensions;
using Orchard.Caching.Services;


namespace Laser.Orchard.StartupConfig.Services {
    [OrchardFeature("Laser.Orchard.StartupConfig.WebApiProtection")]
    public class ApiKeyService : IApiKeyService {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private HttpRequest _request;
        public ILogger Logger;
        private ICacheStorageProvider _cacheStorage;
        private readonly IApiKeySettingService _apiKeySettingService;

        // we use this constant array for trimming URI segments
        private static char[] _slash = { '/' };

        public ApiKeyService(ShellSettings shellSettings, IOrchardServices orchardServices, ICacheStorageProvider cacheManager, IApiKeySettingService apiKeySettingService) {
            _apiKeySettingService = apiKeySettingService;
            _shellSettings = shellSettings;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            _cacheStorage = cacheManager;

            _defaultApplication = new Lazy<ExternalApplication>(() => {
                var name = "";
                while (name.Length < 22)
                    name += _shellSettings.Name;
                var apikey = Convert.ToBase64String(
                    EncryptStringToBytes_Aes(
                        name,
                        _shellSettings.EncryptionKey.ToByteArray(),
                        Encoding.UTF8.GetBytes(string.Format("{0}{0}", DateTime.UtcNow.ToString("ddMMyyyy").Substring(0, 8)))),
                    Base64FormattingOptions.None);
                return new ExternalApplication {
                    Name = "DefaultApplication",
                    ApiKey = apikey,
                    EnableTimeStampVerification = true,
                    Validity = 5
                };
            });
        }

        // surpisingly, we should cache the CurrentSite
        private ProtectionSettingsPart _currentSettings;
        private ProtectionSettingsPart CurrentSettings {
            get {
                if (_currentSettings == null) {
                    _currentSettings = _orchardServices
                        .WorkContext.CurrentSite.As<ProtectionSettingsPart>();
                }
                return _currentSettings;
            }
        }

        public string ValidateRequestByApiKey(string additionalCacheKey, bool protectAlways = false) {
            _request = HttpContext.Current.Request;
            if (additionalCacheKey != null) {
                return additionalCacheKey;
            }

            bool check = false;
            if (protectAlways == false) {

                if (string.IsNullOrWhiteSpace(CurrentSettings.ProtectedEntries)) {
                    return additionalCacheKey; // non ci sono entries da proteggere
                }
                var protectedControllers = CurrentSettings.ProtectedEntries.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var area = _request.RequestContext.RouteData.Values["area"];
                var controller = _request.RequestContext.RouteData.Values["controller"];
                var action = _request.RequestContext.RouteData.Values["action"];
                var entryToVerify = "";
                if (action == null) {
                    // caso che si verifica con le web api (ApiController)
                    entryToVerify = string.Format("{0}.{1}", area, controller);
                } else {
                    // caso che si verifica con i normali Controller
                    entryToVerify = string.Format("{0}.{1}.{2}", area, controller, action);
                }
                if (protectedControllers.Contains(entryToVerify, StringComparer.InvariantCultureIgnoreCase)) {
                    check = true;
                }
            } else {
                check = true;
            }

            if (check == true) {

                var authorized = false;
                var myApiChannel = _request.QueryString["ApiChannel"] ?? _request.Headers["ApiChannel"];

                // New validation methods for API using authorized websites or IP addresses.
                // These new methods require the client to have sent us a valid ApiChannel. On the other hand,
                // if we get a channel, we will attempt to test a configuration for it
                if (!string.IsNullOrWhiteSpace(myApiChannel)) {
                    var app = CurrentSettings.ExternalApplicationList.ExternalApplications
                        .FirstOrDefault(ea => ea.Name.Equals(myApiChannel, StringComparison.OrdinalIgnoreCase));
                    // Even if an ApiChannel is sent, if no application is configured for it, no test is made. 
                    // At the same time, there is, purposefully, no fallback condition: if an ApiChannel is sent, 
                    // and the test for the corresponding configuration fails, there is no fallback test being
                    // performed to recover.
                    if (app != null) {
                        switch (app.ValidationType) {
                            case ApiValidationTypes.ApiKey:
                                // This replicates the "legacy" validation logic
                                authorized = TestApiKeyForChannel(myApiChannel);
                                break;
                            case ApiValidationTypes.Website:
                                authorized = CheckReferer(app);
                                break;
                            case ApiValidationTypes.IpAddress:
                                authorized = CheckIpAddress(app);
                                break;
                            default:
                                // invalid condition: this is really a sanity check where the code should never fall
                                authorized = false;
                                Logger.Error("Invalid ValidationType set for ExternalApplication " + app.Name);
                                break;
                        }
                    }
                    else { 
                        authorized = false;
                        Logger.Error("Impossible to identify configuration for ApiChannel " + myApiChannel);
                    }
                }
                else {
                    // The fallback condition in case no ApiChannel has been sent is to imply a default
                    // Api Channel, and test whether the client has sent valid apikey and akiv. This is
                    // the legacy situation we are still supporting: ApiChannel used to alway be null.
                    authorized = TestApiKeyForChannel(myApiChannel);
                }

                additionalCacheKey = authorized ? "AuthorizedApi" : "UnauthorizedApi";
            }
            return additionalCacheKey;
        }

        private bool TestApiKeyForChannel(string myApiChannel) {
            var myApikey = _request.QueryString["ApiKey"] ?? _request.Headers["ApiKey"];
            var myAkiv = _request.QueryString["AKIV"] ?? _request.Headers["AKIV"];
            return TryValidateKey(
                myApikey, myAkiv,
                (_request.QueryString["ApiKey"] != null && _request.QueryString["clear"] != "false"),
                myApiChannel);
        }

        private bool CheckReferer(ExternalApplication app) {
            // See the ValidationAttributes of the ExternalApplication class for a discussion
            // on the validation of the string used for URL Referer configuration.
            var currentReferer = _request.ServerVariables["HTTP_REFERER"];
            if (string.IsNullOrWhiteSpace(currentReferer)) {
                Logger.Error("Impossible to validate and empty Referer.");
                return false;
            }

            var websites = app.ApiKey.Split(',');
            if (websites.Any()) {
                var refererUri = new Uri(currentReferer);
                foreach (var website in websites) {
                    // The test here cannot simply be a comparison between the strings.
                    var websiteUri = new Uri(website);
                    // Compare uris by Scheme (http/https), host, port
                    if (Uri.Compare(refererUri, websiteUri, UriComponents.SchemeAndServer, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0) {
                        // Compare uris by segments:
                        // https://learn.microsoft.com/en-us/dotnet/api/system.uri.segments?view=netframework-4.8.1
                        // the segments from the website (the one configured in the application) must match exactly,
                        // also in the same order, the segments from the referer. Note that because of how segments are
                        // defined/built by the framework, the last significant one for us may actually be different,
                        // since it may have a trailing '/' character.
                        if (refererUri.Segments.Length >= websiteUri.Segments.Length) {
                            int i = 0;
                            for (; i < websiteUri.Segments.Length; i++) {
                                var webSegment = websiteUri.Segments[i].TrimEnd(_slash);
                                var refSegment = refererUri.Segments[i].TrimEnd(_slash);
                                if (!string.Equals(webSegment, refSegment, StringComparison.OrdinalIgnoreCase)) {
                                    // If the segments don't match, break out and stop testing this website
                                    // for the referer
                                    break;
                                }
                            }
                            if (i == websiteUri.Segments.Length) {
                                // All segments matched, so the referer matches this configured website
                                return true;
                            }
                        }
                    }
                }
            }
            // We found no configured website that matched the referer
            Logger.Error(string.Format("Impossible to validate Referer {0} for Application {1}.", currentReferer, app.Name));
            return false;
        }

        private bool CheckIpAddress(ExternalApplication app) {
            var currentIp = _request.ServerVariables["REMOTE_ADDR"];
            if (string.IsNullOrWhiteSpace(currentIp)) {
                // sanity check
                Logger.Error("Impossible to validate and empty IP.");
                return false;
            }
            
            var ips = app.ApiKey.Split(',');
            // If no ip is specified
            if (ips.Length == 0) {
                Logger.Error(string.Format("No IP is configured for {0}.", app.Name));
                return false;
            }

            foreach (var ip in ips) {
                if (ip.Equals(currentIp)) {
                    return true;
                }

                if (ip.EndsWith("*")) {
                    // Change current ip to match something like "1.2.3.*".
                    var currentMask = currentIp.Substring(0, currentIp.LastIndexOf('.') + 1) + "*";
                    if (ip.Equals(currentMask)) {
                        return true;
                    }
                }
            }

            // We found no configured IP that matched the caller
            Logger.Error(string.Format("Impossible to validate caller IP {0} for Application {1}.", currentIp, app.Name));
            return false;
        }

        public string GetValidApiKey(string sIV, bool useTimeStamp = false) {
            string key = "";
            byte[] mykey;
            byte[] myiv = Convert.FromBase64String(sIV);
            try {
                mykey = _apiKeySettingService.EncryptionKeys("TheDefaultChannel").ToByteArray();

                var defaulApp = DefaultApplication;
                string aux = defaulApp.ApiKey;
                if (useTimeStamp) {
                    Random rnd = new Random();
                    aux += ":" + ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString() + ":" + rnd.Next(1000000).ToString();
                }

                byte[] encryptedAES = EncryptStringToBytes_Aes(aux, mykey, myiv);
                key = Convert.ToBase64String(encryptedAES);
            } catch {
                // ignora volutamente qualsiasi errore e restituisce una stringa vuota
            }
            return key;
        }

        private Lazy<ExternalApplication> _defaultApplication;
        private ExternalApplication DefaultApplication {
            get { return _defaultApplication.Value; }
        }


        private bool TryValidateKey(string token, string akiv, bool clearText, string channel = "TheDefaultChannel") {
            string cacheKey;
            _request = HttpContext.Current.Request;
            try {
                byte[] mykey;
                mykey = _apiKeySettingService.EncryptionKeys(channel).ToByteArray();
                byte[] myiv = Convert.FromBase64String(akiv);
                if (String.IsNullOrWhiteSpace(token)) {
                    Logger.Error("Empty Token");
                    return false;
                }
                string key = token;
                if (!clearText) {
                    var encryptedAES = Convert.FromBase64String(token);
                    key = DecryptStringFromBytes_Aes(encryptedAES, mykey, myiv);
                    //key = aes.Decrypt(token, mykey, myiv);
                } else {
                    var encryptedAES = EncryptStringToBytes_Aes(token, mykey, myiv);
                    var base64EncryptedAES = Convert.ToBase64String(encryptedAES, Base64FormattingOptions.None);
                    //var encrypted = aes.Crypt(token, mykey, myiv);
                    if (_request.QueryString["unmask"] == "true") {
                        HttpContext.Current.Response.Clear();
                        HttpContext.Current.Response.Write("Encoded: " + HttpContext.Current.Server.UrlEncode(base64EncryptedAES) + "<br/>");
                        HttpContext.Current.Response.Write("Clear: " + base64EncryptedAES);
                        HttpContext.Current.Response.End();
                    }
                }
                // test if key has a time stamp
                var tokens = key.Split(':');
                var pureKey = tokens[0];
                int unixTimeStamp, unixTimeStampNow;
                unixTimeStamp = 0;
                unixTimeStampNow = ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                if (tokens.Length >= 2) {
                    unixTimeStamp = Convert.ToInt32(tokens[1]);
                }

                var item = CurrentSettings.ExternalApplicationList.ExternalApplications.FirstOrDefault(x => x.ApiKey.Equals(pureKey));
                if (item == null) {
                    item = DefaultApplication.ApiKey.Equals(pureKey) ? DefaultApplication : item; //this may be a test request
                }
                if (item == null) {
                    Logger.Error("Decrypted key not found: key = " + key);
                    return false;
                }

                var floorLimit = Math.Abs(unixTimeStampNow - unixTimeStamp);
                if (item.EnableTimeStampVerification) {
                    cacheKey = String.Concat(_shellSettings.Name, token);
                    if (_cacheStorage.Get<object>(cacheKey) != null) {
                        Logger.Error("cachekey duplicated: key = " + key);
                        return false;
                    }
                    if (floorLimit > ((item.Validity > 0 ? item.Validity : 5)/*minutes*/ * 60)) {
                        Logger.Error("Timestamp validity expired: key = " + key);
                        return false;
                    } else {
                        _cacheStorage.Put(cacheKey, "", new TimeSpan(0, item.Validity > 0 ? item.Validity : 5, 0));
                    }
                }
                return true;
            } catch (Exception ex) {
                Logger.Error("Exception: " + ex.Message);
                return false;
            }
        }


        private byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV) {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = CreateCryptoService(Key, IV)) {

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream()) {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV) {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = CreateCryptoService(Key, IV)) {
                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText)) {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        private AesCryptoServiceProvider CreateCryptoService(byte[] key, byte[] iv) {
            AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider();
            aesAlg.Key = key;
            aesAlg.IV = iv;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            return aesAlg;
        }
    }
}