using Orchard;
using Orchard.Environment.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Laser.Orchard.Commons.Helpers;
using Orchard.Logging;


namespace Laser.Orchard.StartupConfig.Services {
    public interface ICsrfTokenHelper : IDependency {
        string GenerateCsrfTokenFromAuthToken(string authToken);
        bool DoesCsrfTokenMatchAuthToken();
    }
    public class CsrfTokenHelper : ICsrfTokenHelper {
       private readonly ShellSettings _shellSettings;
       private readonly IOrchardServices _orchardServices;
       public ILogger _Logger { get; set; }
       public CsrfTokenHelper(ShellSettings shellSettings, IOrchardServices orchardServices) {
            _shellSettings = shellSettings;
            ConstantSalt = _shellSettings.EncryptionKey;
            _orchardServices = orchardServices;
            _Logger = NullLogger.Instance;
        }
        private string ConstantSalt { get; set; }//=  var settings = loader.LoadSettings().First() "<ARandomString>";

        public string GenerateCsrfTokenFromAuthToken(string authToken) {
            return GenerateCookieFriendlyHash(authToken);
        }

        public bool DoesCsrfTokenMatchAuthToken() {
            if (HttpContext.Current.Request.Headers.GetValues("X-XSRF-TOKEN") != null) {
                var csrfToken = HttpContext.Current.Request.Headers.GetValues("X-XSRF-TOKEN").FirstOrDefault();
                var authCookie = HttpContext.Current.Request.Cookies[".ASPXAUTH"];
                if (authCookie == null) return false;
                var authToken = authCookie.Value;
                //   var csrfToken = HttpContext.Current.Request.Cookies["XSRF-TOKEN"].Value;
                //   var csrfToken = context.Request.Headers.GetValues("X-XSRF-TOKEN").FirstOrDefault();

                if (String.IsNullOrEmpty(csrfToken)) {
                    return false;// throw new UnauthorizedAccessException("Unauthorized Method");
                }
                var currentUser = _orchardServices.WorkContext.CurrentUser;
                if (currentUser == null) {
                    throw new UnauthorizedAccessException("Unauthorized User");
                }
                if (csrfToken != GenerateCookieFriendlyHash(authToken)) {
#if DEBUG
                    _Logger.Error("X-XSRF-TOKEN" + GenerateCookieFriendlyHash(authToken));            
#endif
                    return false;
                   // throw new UnauthorizedAccessException("Unauthorized Method");
                }
                return csrfToken == GenerateCookieFriendlyHash(authToken);
            }
            else
                return false;
        }

        private string GenerateCookieFriendlyHash(string authToken) {
            return Esegui_HMACSHA512(authToken, ConstantSalt);
        }


        private string Esegui_HMACSHA512(string message, string key) {
            return message.HMACSHA512(key);

        }

    }

}
